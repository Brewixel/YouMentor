using Application.Core;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Time.Testing;
using Polly;
using Testcontainers.PostgreSql;

namespace Application.IntegrationTests.Infrastructure;

public class IntegrationTestBase : IAsyncLifetime
{
	private string ConnectionString { get; set; } = null!;

	protected static readonly DateTimeOffset DefaultTime = new (
		2026, 7, 20,
		10, 0, 0,
		TimeSpan.Zero);

	private PostgreSqlContainer _dbContainer = null!;

	public async Task InitializeAsync()
	{
		_dbContainer = new PostgreSqlBuilder("postgres:18-alpine").Build();
		await _dbContainer.StartAsync();
		ConnectionString = _dbContainer.GetConnectionString();

		await using var context = BuildContext();
		await context.Database.MigrateAsync();
	}

	public async Task DisposeAsync()
	{
		await _dbContainer.StopAsync();
	}

	protected AppDbContext BuildContext(params IInterceptor[] interceptors)
	{
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseNpgsql(ConnectionString)
			.UseSnakeCaseNamingConvention()
			.AddInterceptors(interceptors)
			.Options;
		var context = new AppDbContext(options);
		return context;
	}

	protected async Task<Session?> GetSessionAsync(Guid id)
	{
		await using var context = BuildContext();
		return await context.Sessions.FirstOrDefaultAsync(x => x.Id == id);
	}

	protected FakeCurrentUser GetFakeUser(Guid? userId = null)
	{
		return new FakeCurrentUser
		{
			UserId = userId ?? Guid.NewGuid(),
		};
	}

	protected FakeCurrentUser GetUnauthorizedUser()
	{
		return new FakeCurrentUser
		{
			UserId = null,
		};
	}

	protected FakeTimeProvider GetTimeProvider()
	{
		return new FakeTimeProvider(DefaultTime);
	}

	protected async Task<Guid> CreateSessionAsync(SessionCreationConfig? config = null)
	{
		config ??= new SessionCreationConfig();
		var mentorId = config.MentorId ?? Guid.NewGuid();
		var currentTime = config.CurrentTime ?? DefaultTime;
		var startTime = config.StartTime ?? currentTime.AddDays(1);
		var duration = TimeSpan.FromHours(1);

		var creationResult = Session.Create(currentTime, mentorId, startTime, duration);
		if (!creationResult.IsSuccess)
			throw new InvalidOperationException($"Session creation failed: {creationResult.ErrorInfo!.Message}");

		var session = creationResult.Value!;

		if (config.MakeCanceled)
		{
			var cancellationResult = session.Cancel(currentTime);
			if (!cancellationResult.IsSuccess)
				throw new InvalidOperationException($"Session cancellation failed: {cancellationResult.ErrorInfo!.Message}");
		}

		await using var setupContext = BuildContext();
		setupContext.Sessions.Add(session);
		await setupContext.SaveChangesAsync();

		return session.Id;
	}

	protected async Task BookSessionAsync(Guid sessionId, FakeTimeProvider timeProvider, FakeCurrentUser studentUser,
		CancellationToken ct)
	{
		await using var concurrentContext = BuildContext();
		var session = await  concurrentContext.Sessions.FirstOrDefaultAsync(x => x.Id == sessionId, ct);
		if (session == null)
		{
			throw new InvalidOperationException($"Session with id {sessionId} not found");
		}

		var bookingResult = session.Book(timeProvider.GetUtcNow(), studentUser.UserId!.Value);
		if (!bookingResult.IsSuccess)
		{
			throw new InvalidOperationException(
				$"Concurrent booking failed: {bookingResult.ErrorInfo?.Message}");
		}

		await concurrentContext.SaveChangesAsync(ct);
	}

	protected async Task CancelSessionAsync(Guid sessionId, FakeTimeProvider timeProvider, CancellationToken ct)
	{
		await using var concurrentContext = BuildContext();
		var session = await  concurrentContext.Sessions.FirstOrDefaultAsync(x => x.Id == sessionId, ct);
		if (session == null)
		{
			throw new InvalidOperationException($"Session with id {sessionId} not found");
		}

		var cancellingResult = session.Cancel(timeProvider.GetUtcNow());
		if (!cancellingResult.IsSuccess)
		{
			throw new InvalidOperationException(
				$"Concurrent cancelling failed: {cancellingResult.ErrorInfo?.Message}");
		}
		await concurrentContext.SaveChangesAsync(ct);
	}

	protected ResiliencePipeline GetEmptyPipeline()
	{
		return new ResiliencePipelineBuilder().Build();
	}

	protected ResiliencePipeline GetConcurrencyPipeline()
	{
		return new ResiliencePipelineBuilder()
			.AddDatabaseConcurrencyRetry(0)
			.Build();
	}
}
