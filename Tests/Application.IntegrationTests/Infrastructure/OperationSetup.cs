using Infrastructure.Persistence;

namespace Application.IntegrationTests.Infrastructure;

public sealed class OperationSetup<TCommand, THandler> : IAsyncDisposable
{
	public required TCommand Command { get; init; }
	public required THandler Handler { get; init; }
	public required AppDbContext Context { get; init; }
	public required FakeCurrentUser CurrentUser { get; init; }
	public required TimeProvider TimeProvider { get; init; }
	public FakeLogger<THandler>? Logger { get; init; }

	public async ValueTask DisposeAsync()
	{
		await Context.DisposeAsync();
	}
}
