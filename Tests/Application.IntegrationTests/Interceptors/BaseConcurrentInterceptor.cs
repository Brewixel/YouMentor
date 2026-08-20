using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Application.IntegrationTests.Interceptors;

public class BaseConcurrentInterceptor : SaveChangesInterceptor
{
	public int SavesCount { get; private set; }

	protected void RegisterSaveAttempt()
	{
		SavesCount++;
	}
}
