using System.Text.Json;
using Application.Interfaces;
using Application.Signals;
using Contracts;
using Microsoft.EntityFrameworkCore;

namespace Api.Workers;

public class OutboxWorker(
		OutboxSignal signal,
		IServiceScopeFactory scopeFactory,
		TimeProvider timeProvider,
		ILogger<OutboxWorker> logger
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
			var message = await dbContext.OutboxMessages
				.FromSql($"""
					SELECT * FROM outbox_messages
					WHERE sent_at IS NULL
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

			await notificationSender.SendAsync(payload, ct);
			message.SentAt = timeProvider.GetUtcNow();
			await dbContext.SaveChangesAsync(ct);
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
