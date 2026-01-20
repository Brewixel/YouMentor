using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Application.Core;

public class AndSpecification<T>(Specification<T> leftSpec, Specification<T> rightSpec)
	: Specification<T>
{
	public override Expression<Func<T, bool>> ToExpression()
	{
		var leftExpr = leftSpec.ToExpression();
		var rightExpr = rightSpec.ToExpression();

		var parameter = leftExpr.Parameters[0];
		var parameterReplacer = new ParameterReplacer(rightExpr.Parameters[0], parameter);

		var rightExprFixed = parameterReplacer.Visit(rightExpr.Body) ?? Expression.Empty();
		var body = Expression.AndAlso(leftExpr.Body, rightExprFixed);

		return Expression.Lambda<Func<T, bool>>(body, parameter);
	}
}
