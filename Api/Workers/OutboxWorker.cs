using System.Text.Json;
using Application.Core;
using Application.Interfaces;
using Application.Signals;
using Contracts;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace Api.Workers;

public class OutboxWorker(
		OutboxSignal signal,
		IServiceScopeFactory scopeFactory,
		TimeProvider timeProvider,
		ILogger<OutboxWorker> logger,
		[FromKeyedServices(Consts.Pipelines.OutboxSend.Name)] ResiliencePipeline sendPipeline
	) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await ProcessPendingMessagesAsync(stoppingToken);

		while (!stoppingToken.IsCancellationRequested)
		{
			using var waitCts =
				CancellationTokenSource.CreateLinkedTokenSource(
					stoppingToken);

			waitCts.CancelAfter(TimeSpan.FromSeconds(30));

			try
			{
				bool hasData = await signal.Reader
					.WaitToReadAsync(waitCts.Token);

				if (!hasData)
					break;

				while (signal.Reader.TryRead(out _))
				{ }
			}
			catch (OperationCanceledException)
				when (!stoppingToken.IsCancellationRequested)
			{ }

			await ProcessPendingMessagesAsync(stoppingToken);
		}
	}

	private async Task ProcessPendingMessagesAsync(
		CancellationToken ct)
	{
		try
		{
			await using var scope = scopeFactory.CreateAsyncScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
			await using var transaction = await  dbContext.Database.BeginTransactionAsync(ct);
			var message = await dbContext.OutboxMessages
				.FromSql($"""
					SELECT * FROM outbox_messages
					WHERE sent_at IS NULL
					ORDER BY created_at, Id
					LIMIT 1
					FOR UPDATE SKIP LOCKED
				""")
				.FirstOrDefaultAsync(ct);

			if (message is null)
				return;
			var notificationSender = scope.ServiceProvider.GetRequiredService<INotificationSender>();
			var payload = JsonSerializer.Deserialize<SendNotification>(message.Payload);
			if (payload is null)
				throw new InvalidOperationException("Payload is null");

			await sendPipeline.ExecuteAsync(
				async sendToken => await notificationSender.SendAsync(payload, sendToken),
				ct);
			message.SentAt = timeProvider.GetUtcNow();
			await dbContext.SaveChangesAsync(ct);
			await transaction.CommitAsync(ct);
		}
		catch (OperationCanceledException)
			when (ct.IsCancellationRequested)
		{
			throw;
		}
		catch (Exception ex)
		{
			logger.LogError(
				ex,
				"Failed to process outbox messages");
		}
	}
}
