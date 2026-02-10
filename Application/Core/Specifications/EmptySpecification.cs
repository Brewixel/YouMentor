using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Application.Core.Specifications;

public class EmptySpecification<T> : Specification<T>
{
	public override Expression<Func<T, bool>> ToExpression()
	{
		return _ => true;
	}
}
