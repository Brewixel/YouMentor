namespace Application.Core;

public static class Consts
{
	public static class PipelineNames
	{
		public const string DatabaseConcurrency = "database-concurrency";
	}

	public static class PipelineProps
	{
		public const int MaxRetryAttempts = 3;
	}
}
