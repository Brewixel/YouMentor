using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;

namespace Application.Core;

public static class DatabaseConcurrencyPipeline
{
	public static ResiliencePipelineBuilder AddDatabaseConcurrencyRetry(
		this ResiliencePipelineBuilder builder, int delayMilliseconds)
	{
		return builder.AddRetry(new RetryStrategyOptions
		{
			ShouldHandle = new PredicateBuilder()
				.Handle<DbUpdateConcurrencyException>(),
			MaxRetryAttempts = Consts.Pipelines.DatabaseConcurrency.MaxRetryAttempts,
			Delay = TimeSpan.FromMilliseconds(delayMilliseconds)
		});
	}
}
