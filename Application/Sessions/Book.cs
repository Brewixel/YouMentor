using Application.Core;
using Application.Interfaces;
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
		[FromKeyedServices(Consts.PipelineNames.DatabaseConcurrency)]
			ResiliencePipeline concurrencyPipeline
		) : IRequestHandler<Command, Result>
	{
		public async Task<Result> Handle(Command request, CancellationToken ct)
		{
			var studentId = currentUser.UserId;
			if (studentId == null)
				return Result.Unauthorized();

			try
			{
				return await concurrencyPipeline.ExecuteAsync(
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

						try
						{
							await context.SaveChangesAsync(pipelineCancellationToken);
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
					Consts.PipelineProps.MaxRetryAttempts);

				return Result.Failure(
					"Unable to book session, please try again later");
			}
		}
	}
}
