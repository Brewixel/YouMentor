using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace Application.Core.Specifications;

public abstract class Specification<T>
{
	public static Specification<T> Empty => new EmptySpecification<T>();

	public abstract Expression<Func<T, bool>> ToExpression();

	public Specification<T> And(Specification<T> other)
	{
		return new AndSpecification<T>(this, other);
	}


	public static implicit operator Expression<Func<T, bool>>(Specification<T> spec)
	{
		return spec.ToExpression();
	}
}
