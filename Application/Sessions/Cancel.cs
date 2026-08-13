using Application.Core;
using Application.Interfaces;
using Domain.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;

namespace Application.Sessions;

public class Cancel
{
	public class Command : IRequest<Result>
	{
		public required Guid SessionId { get; init; }
	}

	public class Handler(
		IAppDbContext context,
		ICurrentUser currentUser,
		TimeProvider timeProvider,
		ILogger<Handler> logger,
		[FromKeyedServices(Consts.PipelineNames.DatabaseConcurrency)]
			ResiliencePipeline concurrencyPipeline
		) : IRequestHandler<Command, Result>
	{
		public async Task<Result> Handle(Command request, CancellationToken ct)
		{
			var mentorId = currentUser.UserId;
			if (mentorId == null)
				return Result.Unauthorized();

			try
			{
				return await concurrencyPipeline.ExecuteAsync(
					async pipelineCancellationToken =>
					{
						var session = await context.Sessions.FirstOrDefaultAsync(
							x => x.Id == request.SessionId, pipelineCancellationToken);

						if (session == null)
							return Result.NotFound($"Session with id {request.SessionId} not found");

						if (session.MentorId != mentorId)
							return Result.Forbidden();

						var currentTime = timeProvider.GetUtcNow();
						var cancelingResult = session.Cancel(currentTime);
						if (!cancelingResult.IsSuccess)
							return cancelingResult;

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
			catch (DbUpdateConcurrencyException ex)
			{
				logger.LogWarning(
					ex,
					"Unable to cancel session {SessionId} for mentor {MentorId} " +
					"after {MaxRetries} retries",
					request.SessionId,
					mentorId.Value,
					Consts.PipelineProps.MaxRetryAttempts);

				return Result.Failure(
					"Unable to cancel session, please try again later");
			}
		}
	}
}
