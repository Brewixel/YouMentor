using Application.Interfaces;
using Domain.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Sessions;

public class Cancel
{
	public class Command : IRequest<Result>
	{
		public required Guid SessionId { get; init; }
	}

	public class Handler(
		IAppDbContext context,
		ICurrentUser currentUser) : IRequestHandler<Command, Result>
	{
		public async Task<Result> Handle(Command request, CancellationToken ct)
		{
			var mentorId = currentUser.UserId;
			if (mentorId == null)
				return Result.Unauthorized();

			var session = await context.Sessions.FirstOrDefaultAsync(x => x.Id == request.SessionId, ct);

			if (session == null)
				return Result.NotFound($"Session with id {request.SessionId} not found");

			if (session.MentorId != mentorId)
				return Result.Forbidden();

			var cancelingResult = session.Cancel();
			if (!cancelingResult.IsSuccess)
				return cancelingResult;

			await context.SaveChangesAsync(ct);
			return Result.Success();
		}
	}
}
