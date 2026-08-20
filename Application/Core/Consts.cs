namespace Application.Core;

public static class Consts
{
	public static class Pipelines
	{
		public static class DatabaseConcurrency
		{
			public const string Name = "database-concurrency";
			public const int MaxRetryAttempts = 2;
			public const int DelayMilliseconds = 25;
		}
	}
}
