using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using Application.Core;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Sessions
{
	public class Create
	{
		public class Command : IRequest<Result<Guid>>
		{
			public CreateSessionDto Session { get; set; }
		}

		public class Handler(IAppDbContext _context) : IRequestHandler<Command, Result<Guid>>
		{
			public async Task<Result<Guid>> Handle(Command request, CancellationToken cancellationToken)
            {
				var session = new Session(request.Session.MentorId, request.Session.StartTime, request.Session.Duration);
				_context.Sessions.Add(session);
				var result = await _context.SaveChangesAsync(cancellationToken) > 0;

				if (!result)
					return Result<Guid>.Failure("Failed to create session");

				return Result<Guid>.Success(session.Id);
			}
        }
	}
}
