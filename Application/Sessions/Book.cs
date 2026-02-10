using Application.Interfaces;
using Domain.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Sessions;

public class Book
{
	public class Command : IRequest<Result>
	{
		public Guid SessionId { get; set; }
		public Guid StudentId { get; set; }
	}

	public class Handler(IAppDbContext context) : IRequestHandler<Command, Result>
	{
		public async Task<Result> Handle(Command request, CancellationToken ct)
		{
			var session = await context.Sessions.FirstOrDefaultAsync(x => x.Id == request.SessionId, ct);
			if (session == null)
				return Result.NotFound($"Session with id {request.SessionId} not found");

			var bookingResult = session.Book(request.StudentId);

			if (!bookingResult.IsSuccess)
				return bookingResult;

			var changedRows = await context.SaveChangesAsync(ct);
			if (changedRows == 0)
				return Result.Failure($"Failed to update session");

			return Result.Success();
		}
	}
}
