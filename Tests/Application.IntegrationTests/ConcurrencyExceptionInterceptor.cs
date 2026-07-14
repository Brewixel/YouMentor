using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Application.IntegrationTests;

public class ConcurrencyExceptionInterceptor(int maxExceptionsCount) : SaveChangesInterceptor
{
	private int _exceptionsCount = 0;

	public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
		DbContextEventData eventData,
		InterceptionResult<int> result,
		CancellationToken cancellationToken = default)
	{
		if (_exceptionsCount < maxExceptionsCount)
		{
			_exceptionsCount++;
			throw new DbUpdateConcurrencyException("Simulated concurrency conflict");
		}

		return base.SavingChangesAsync(eventData, result, cancellationToken);
	}
}
