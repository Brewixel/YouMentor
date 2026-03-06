namespace Api.Middlewares;

public static class RequestTimingMiddlewareExtensions
{
	public static IApplicationBuilder UseRequestTiming(this IApplicationBuilder builder)
	{
		return builder.UseMiddleware<RequestTimingMiddleware>();
	}
}
