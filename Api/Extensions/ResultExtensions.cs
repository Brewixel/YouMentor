using Domain.Results;

namespace Api.Extensions;

public static class ResultExtensions
{
	public static IResult ToHttpResult(this Result result)
	{
		if (result.IsSuccess)
			return Results.NoContent();

		return GetHttpError(result.ErrorInfo);
	}

	public static IResult ToHttpResult<T>(this Result<T> result)
	{
		if (result.IsSuccess)
			return Results.Ok(result.Value);

		return GetHttpError(result.ErrorInfo);
	}

	private static IResult GetHttpError(ErrorInfo errorInfo)
	{
		return errorInfo.Type switch
		{
			ErrorType.Validation => TypedResults.BadRequest(errorInfo.Message),
			ErrorType.NotFound => TypedResults.NotFound(errorInfo.Message),
			ErrorType.Conflict => TypedResults.Conflict(errorInfo.Message),
			_ => TypedResults.Problem // Для Failure и остальных
			(
				errorInfo.Message,
				statusCode: 500,
				title: "Server error"
			)
		};
	}
}
