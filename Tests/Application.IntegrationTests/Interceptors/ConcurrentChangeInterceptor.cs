using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Application.IntegrationTests.Interceptors;

public class ConcurrentChangeInterceptor(Func<CancellationToken, Task> action) : BaseConcurrentInterceptor
{
	public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
		DbContextEventData eventData,
		InterceptionResult<int> result,
		CancellationToken cancellationToken = default)
	{
		RegisterSaveAttempt();

		if (SavesCount == 1)
		{
			await action(cancellationToken);
		}

		return await base.SavingChangesAsync(eventData, result, cancellationToken);
	}
}
