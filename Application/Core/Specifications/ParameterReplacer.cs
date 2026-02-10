using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text;

namespace Application.Core.Specifications;

public class ParameterReplacer(ParameterExpression oldParameter, ParameterExpression newParameter)
	: ExpressionVisitor
{
	public override Expression? Visit(Expression? node)
	{
		return node == oldParameter
			? newParameter
			: base.Visit(node);
	}
}
