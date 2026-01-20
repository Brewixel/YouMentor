using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Core;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Extensions;

public static class ResultExtensions
{
	public static Results<Ok<T>, NotFound, BadRequest<string>> ToHttpResult<T>(this Result<T> result)
	{
		if (result == null
			|| result.IsSuccess && result.Value == null)
		{
			return TypedResults.NotFound();
		}

		if (result.IsSuccess && result.Value != null)
			return TypedResults.Ok(result.Value);

		return TypedResults.BadRequest(result.Error);
	}
}
