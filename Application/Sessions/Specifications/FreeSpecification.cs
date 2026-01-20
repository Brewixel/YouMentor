using Application.Core;
using Domain.Entities;
using System.Linq.Expressions;

namespace Application.Sessions.Specifications;

public class FreeSpecification : Specification<Session>
{
	public override Expression<Func<Session, bool>> ToExpression()
	{
		return s => s.Status == SessionStatus.Free;
	}
}
