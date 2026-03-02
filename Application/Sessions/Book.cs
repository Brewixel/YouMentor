using Application.Interfaces;
using Domain.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Sessions;

public class Book
{
	public class Command : IRequest<Result>
	{
		public required Guid SessionId { get; set; }
		public required Guid StudentId { get; set; }
	}

	public class Handler(IAppDbContext context, ILogger<Handler> logger) : IRequestHandler<Command, Result>
	{
		public const int MaxRetries = 3;

		public async Task<Result> Handle(Command request, CancellationToken ct)
		{
			var retries = 0;

			while(retries <= MaxRetries)
			{
				var session = await context.Sessions.FirstOrDefaultAsync(x => x.Id == request.SessionId, ct);
				if (session == null)
					return Result.NotFound($"Session with id {request.SessionId} not found");

				var bookingResult = session.Book(request.StudentId);

				if (!bookingResult.IsSuccess)
					return bookingResult;

				try
				{
					await context.SaveChangesAsync(ct);
					return Result.Success();
				}
				catch(DbUpdateConcurrencyException _)
				{
					logger.LogWarning("Concurrency conflict detected while booking session {SessionId} for student {StudentId}. Retrying...",
						request.SessionId, request.StudentId);

					context.ChangeTracker.Clear();
					retries++;

					var delay = Random.Shared.Next(1, 6) * 10;
					await Task.Delay(delay, ct);
				}
			}

			return Result.Failure("Unable to book session, please try again later");
		}
	}
}
