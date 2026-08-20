using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Application.IntegrationTests.Interceptors;

public class SillyExceptionInterceptor(int maxExceptionsCount) : BaseConcurrentInterceptor
{
	private int _exceptionsCount = 0;

	public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
		DbContextEventData eventData,
		InterceptionResult<int> result,
		CancellationToken cancellationToken = default)
	{
		RegisterSaveAttempt();

		if (_exceptionsCount < maxExceptionsCount)
		{
			_exceptionsCount++;
			throw new DbUpdateConcurrencyException("Simulated concurrency conflict");
		}

		return base.SavingChangesAsync(eventData, result, cancellationToken);
	}
}
