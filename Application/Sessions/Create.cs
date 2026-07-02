using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using Application.Core;
using Application.Interfaces;
using Domain.Entities;
using Domain.Results;

namespace Application.Sessions;

public class Create
{
	public class Command : IRequest<Result<Guid>>
	{
		public required CreateSessionDto Session { get; set; }
	}

	public class Handler(
		IAppDbContext context,
		ICurrentUser currentUser) : IRequestHandler<Command, Result<Guid>>
	{
		public async Task<Result<Guid>> Handle(Command request, CancellationToken ct)
		{
			var mentorId = currentUser.UserId;
			if (mentorId == null)
				return Result<Guid>.Unauthorized();

			var result = Session.Create(
				mentorId.Value,
				request.Session.StartTime,
				DateTime.UtcNow,
				request.Session.Duration
			);

			if (!result.IsSuccess)
				return result.Map(_ => Guid.Empty);

			Session session = result.Value!;

			context.Sessions.Add(session);
			await context.SaveChangesAsync(ct);

			return Result<Guid>.Success(session.Id);
		}
	}
}
