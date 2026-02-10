using Application.Core.Specifications;
using Domain.Entities;
using System.Linq.Expressions;

namespace Application.Sessions.Specifications;

public class ByMentorSpecification(Guid mentorId) : Specification<Session>
{
	public override Expression<Func<Session, bool>> ToExpression()
	{
		return s => s.MentorId == mentorId;
	}
}
