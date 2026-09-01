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

	public static class Notifications
	{
		public const string Source = "youmentor";

		public static class Channels
		{
			public const string InApp = "in-app";
			public const string Email = "email";
			public const string Whatsapp = "whatsapp";
		}
	}
}
