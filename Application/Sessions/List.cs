using Application.Core;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Sessions
{
	public class List
	{
		public class Query : IRequest<Result<List<SessionDto>>> { }

		public class Handler(IAppDbContext _context, SessionMapper _mapper) : IRequestHandler<Query, Result<List<SessionDto>>>
		{
			public async Task<Result<List<SessionDto>>> Handle(Query request, CancellationToken cancellationToken)
			{
				var result = await _context.Sessions.Where(x => x.Status == SessionStatus.Free)
					.AsNoTracking().ToListAsync();
				return Result<List<SessionDto>>.Success(_mapper.SessionToDto(result));
			}
		}
	}
}
