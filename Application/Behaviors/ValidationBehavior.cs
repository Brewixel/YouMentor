using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Domain.Results;
using FluentValidation;
using MediatR;

namespace Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
	: IPipelineBehavior<TRequest, TResponse>
	where TRequest : IRequest<TResponse>
	where TResponse : Result
{
	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		if (!validators.Any())
			return await next();

		var results = await Task.WhenAll(validators
			.Select(x => x.ValidateAsync(request, cancellationToken)));

		var errors = results.SelectMany(x => x.Errors)
			.ToList();

		if (!errors.Any())
			return await next();

		var errorMessage = string.Join(Environment.NewLine, errors.Select(x => x.ErrorMessage));

		var getValidationMethod = typeof(TResponse).GetMethod(
			nameof(Result.Validation),
			BindingFlags.Public | BindingFlags.Static,
			[typeof(string)]
		);

		if (getValidationMethod == null)
			throw new InvalidOperationException("Method Validation not found");

		return (TResponse)getValidationMethod.Invoke(null, [errorMessage])!;
	}
}
