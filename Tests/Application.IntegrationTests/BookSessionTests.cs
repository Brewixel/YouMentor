using Application.Sessions;
using Domain.Entities;
using Domain.Results;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Application.IntegrationTests;

public class BookSessionTests : IntegrationTestBase
{
	[Fact]
	public async Task Handle_Should_BookSession_And_SaveToDatabase()
	{
		// Arrange
		var mentorUser = GetFakeUser();
		var studentUser = GetFakeUser();
		var timeProvider = GetTimeProvider();
		var config = new SessionCreationConfig
		{
			CurrentTime =  timeProvider.GetUtcNow(),
			MentorId = mentorUser.UserId
		};
		var sessionId = await CreateSessionAsync(config);
		await CheckSessionPersisted(sessionId);

		var command = new Book.Command
		{
			SessionId = sessionId,
		};

		await using var setupContext = BuildContext();
		var handler = new Book.Handler(
			setupContext,
			studentUser,
			NullLogger<Book.Handler>.Instance,
			timeProvider);

		// Act
		var result = await handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue(result.ErrorInfo?.Message);

		var session = await GetSessionAsync(sessionId);
		session.Should().NotBeNull();
		session!.Status.Should().Be(SessionStatus.Booked);
		session.StudentId.Should().Be(studentUser.UserId!.Value);
	}

	[Fact]
	public async Task Handle_Should_ReturnUnauthorized_When_StudentUserIsNull()
	{
		// Arrange
		var mentorUser = GetFakeUser();
		var timeProvider = GetTimeProvider();
		var config = new SessionCreationConfig
		{
			CurrentTime =  timeProvider.GetUtcNow(),
			MentorId = mentorUser.UserId
		};
		var sessionId = await CreateSessionAsync(config);
		await CheckSessionPersisted(sessionId);

		var command = new Book.Command
		{
			SessionId = sessionId,
		};

		await using var setupContext = BuildContext();
		var unauthorizedStudent = GetUnauthorizedUser();
		var handler = new Book.Handler(
			setupContext,
			unauthorizedStudent,
			NullLogger<Book.Handler>.Instance,
			timeProvider);

		// Act
		var result = await handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorInfo?.Type.Should().Be(ErrorType.Unauthorized);

		var session = await GetSessionAsync(sessionId);
		session.Should().NotBeNull();
		session!.Status.Should().Be(SessionStatus.Free);
		session.StudentId.Should().BeNull();
	}

	[Fact]
	public async Task Handle_Should_ReturnNotFound_When_SessionDoesNotExist()
	{
		// Arrange
		var studentUser = GetFakeUser();
		var timeProvider = GetTimeProvider();

		var command = new Book.Command
		{
			SessionId = Guid.NewGuid(),
		};

		await using var setupContext = BuildContext();
		var handler = new Book.Handler(
			setupContext,
			studentUser,
			NullLogger<Book.Handler>.Instance,
			timeProvider);

		// Act
		var result = await handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorInfo?.Type.Should().Be(ErrorType.NotFound);

		var session = await GetSessionAsync(command.SessionId);
		session.Should().BeNull();
	}

	[Fact]
	public async Task Handle_Should_ReturnConflict_And_NotModifySession_When_SessionIsCanceled()
	{
		// Arrange
		var mentorUser = GetFakeUser();
		var studentUser = GetFakeUser();
		var timeProvider = GetTimeProvider();
		var config = new SessionCreationConfig
		{
			CurrentTime =  timeProvider.GetUtcNow(),
			MentorId = mentorUser.UserId,
			MakeCanceled =  true
		};
		var sessionId = await CreateSessionAsync(config);
		await CheckSessionPersisted(sessionId, SessionStatus.Canceled);

		var command = new Book.Command
		{
			SessionId = sessionId,
		};

		await using var setupContext = BuildContext();
		var handler = new Book.Handler(
			setupContext,
			studentUser,
			NullLogger<Book.Handler>.Instance,
			timeProvider);

		// Act
		var result = await handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorInfo?.Type.Should().Be(ErrorType.Conflict);

		var session = await GetSessionAsync(command.SessionId);
		session.Should().NotBeNull();
		session.Status.Should().Be(SessionStatus.Canceled);
		session.StudentId.Should().BeNull();
	}

	/*

	[Fact]
	public async Task Handle_Should_RetryAndSave_When_RetriesCountLessOrEqualThanMax()
	{
	}

	[Fact]
	public async Task Handle_Should_ReturnFailure_When_RetriesCountMoreThanMax()
	{
	}
	*/

	private BookSetup BuildBookSetup(
		Guid sessionId,
		FakeCurrentUser currentUser,
		TimeProvider? timeProvider = null,
		FakeLogger<Book.Handler>? logger = null,
		IInterceptor[]? contextInterceptors = null)
	{
		timeProvider ??= GetTimeProvider();
		contextInterceptors ??= [];

		ILogger<Book.Handler> abstractLogger =
			logger != null
				? logger
				: NullLogger<Book.Handler>.Instance;

		var command = new Book.Command
		{
			SessionId = sessionId,
		};

		var setupContext = BuildContext(contextInterceptors);

		var handler = new Book.Handler(
			setupContext,
			currentUser,
			abstractLogger,
			timeProvider);

		return new BookSetup
		{
			Command = command,
			Handler = handler,
			Context = setupContext,
			Logger = logger,
			CurrentUser = currentUser,
			TimeProvider = timeProvider,
		};
	}

	public sealed class BookSetup : IAsyncDisposable
	{
		public required Book.Command Command { get; init; }
		public required Book.Handler Handler { get; init; }
		public required AppDbContext Context { get; init; }
		public required FakeCurrentUser CurrentUser { get; init; }
		public required TimeProvider TimeProvider { get; init; }
		public FakeLogger<Book.Handler>? Logger { get; init; }

		public async ValueTask DisposeAsync()
		{
			await Context.DisposeAsync();
		}
	}
}
