using Application.Core;
using Application.Core.Specifications;
using Application.Interfaces;
using Application.Sessions.Specifications;
using Domain.Entities;
using Domain.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Sessions;

public class List
{
	public class Query : IRequest<Result<List<SessionDto>>>
	{
		public Guid? MentorId { get; init; }
		public bool OnlyFree { get; init; }
	}

	public class Handler(IAppDbContext _context, SessionMapper _mapper) : IRequestHandler<Query, Result<List<SessionDto>>>
	{
		public async Task<Result<List<SessionDto>>> Handle(Query request, CancellationToken ct)
		{
			var spec = BuildSpecification(request);

			var result = await _context.Sessions
				.Where(spec)
				.AsNoTracking()
				.ToListAsync(ct);

			return Result<List<SessionDto>>.Success(
				_mapper.SessionToDto(result));
		}

		private Specification<Session> BuildSpecification(Query request)
		{
			var spec = Specification<Session>.Empty;

			if (request.OnlyFree)
			{
				spec = spec.And(new FreeSpecification());
			}

			if (request.MentorId != null)
			{
				spec = spec.And(new ByMentorSpecification(request.MentorId.Value));
			}

			return spec;
		}
	}
}
