using Application.Sessions;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Time.Testing;
using Testcontainers.PostgreSql;

namespace Application.IntegrationTests;

public class IntegrationTestBase : IAsyncLifetime
{
	public string ConnectionString { get; set; } = null!;

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

	protected async Task CheckSessionPersisted(Guid sessionId, SessionStatus status = SessionStatus.Free, DateTimeOffset? startTime = null)
	{
		await using var context = BuildContext();
		var session = await context.Sessions.FirstOrDefaultAsync(x => x.Id == sessionId);

		session.Should().NotBeNull();
		session.Status.Should().Be(status);

		if (startTime.HasValue)
			session.StartTime.Should().BeCloseTo(startTime.Value, TimeSpan.FromMilliseconds(1));
	}
}
