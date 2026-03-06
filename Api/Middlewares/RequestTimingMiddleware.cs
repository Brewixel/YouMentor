using System.Diagnostics;

namespace Api.Middlewares;

public class RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
{
	public async Task InvokeAsync(HttpContext context)
	{
		var stopwatch = new Stopwatch();
		stopwatch.Start();
		await next(context);
		stopwatch.Stop();
		logger.LogInformation($"Request time: {stopwatch.ElapsedMilliseconds} ms");
	}
}
