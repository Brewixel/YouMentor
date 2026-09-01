using Application.Core;
using Application.Interfaces;
using Application.Signals;
using Contracts;
using Domain.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;

namespace Application.Sessions;

public class Book
{
	public class Command : IRequest<Result>
	{
		public required Guid SessionId { get; init; }
	}

	public class Handler(
		IAppDbContext context,
		ICurrentUser currentUser,
		ILogger<Handler> logger,
		TimeProvider timeProvider,
		[FromKeyedServices(Consts.Pipelines.DatabaseConcurrency.Name)]
			ResiliencePipeline concurrencyPipeline,
		IOutboxService outboxService,
		OutboxSignal signal
		) : IRequestHandler<Command, Result>
	{
		public async Task<Result> Handle(Command request, CancellationToken ct)
		{
			var studentId = currentUser.UserId;
			if (studentId == null)
				return Result.Unauthorized();

			Result result;

			try
			{
				result = await concurrencyPipeline.ExecuteAsync(
					async pipelineCancellationToken =>
					{
						var session = await context.Sessions.FirstOrDefaultAsync(
							x => x.Id == request.SessionId,
							pipelineCancellationToken);

						if (session == null)
							return Result.NotFound($"Session with id {request.SessionId} not found");

						var currentTime = timeProvider.GetUtcNow();
						var bookingResult = session.Book(currentTime, studentId.Value);

						if (!bookingResult.IsSuccess)
							return bookingResult;

						outboxService.AddIntent(new SendNotification(
							Guid.NewGuid(),
							Consts.Notifications.Source,
							session.MentorId,
							Consts.Notifications.Channels.InApp,
							$"Session {session.Id} booked by {studentId.Value}",
							currentTime));

						try
						{
							await context.SaveChangesAsync(pipelineCancellationToken);
							signal.Notify();
							return Result.Success();
						}
						catch (DbUpdateConcurrencyException)
						{
							context.ChangeTracker.Clear();
							throw;
						}
					},
					ct
				);
			}
			catch (DbUpdateConcurrencyException exception)
			{
				logger.LogWarning(
					exception,
					"Unable to book session {SessionId} for student {StudentId} " +
					"after {MaxRetries} retries",
					request.SessionId,
					studentId.Value,
					Consts.Pipelines.DatabaseConcurrency.MaxRetryAttempts);

				return Result.Failure(
					"Unable to book session, please try again later");
			}

			return result;
		}
	}
}
