using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Api.ExceptionHandling;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
	public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
	{
		logger.LogError(exception, "Unhandled exception");

		var problemDetails = new ProblemDetails()
		{
			Status = StatusCodes.Status500InternalServerError,
			Title = "Unexpected server error",
			Detail = "An unexpected error has occurred. Please contact your system administrator",
		};

		httpContext.Response.StatusCode = problemDetails.Status.Value;
		await httpContext.Response.WriteAsJsonAsync(problemDetails);

		return true;
	}
}
