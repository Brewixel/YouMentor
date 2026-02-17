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

	public class Handler(IAppDbContext _context) : IRequestHandler<Command, Result<Guid>>
	{
		public async Task<Result<Guid>> Handle(Command request, CancellationToken ct)
		{
			var result = Session.Create(
				request.Session.MentorId,
				request.Session.StartTime,
				DateTime.UtcNow,
				request.Session.Duration
			);

			if (!result.IsSuccess)
				return result.Map(_ => Guid.Empty);

			Session session = result.Value!;

			_context.Sessions.Add(session);
			var saved = await _context.SaveChangesAsync(ct) > 0;

			if (!saved)
				return Result<Guid>.Failure("Failed to create session");

			return Result<Guid>.Success(session.Id);
		}
	}
}
