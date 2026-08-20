using Application.IntegrationTests.Infrastructure;
using Application.IntegrationTests.Interceptors;
using Application.Sessions;
using Domain.Entities;
using Domain.Results;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Polly;
using static Application.Core.Consts;

namespace Application.IntegrationTests.Sessions;

public class BookTests : IntegrationTestBase
{
	[Fact]
	public async Task Should_BookSession_And_SaveToDatabase()
	{
		// Arrange
		var mentorUser = GetFakeUser();
		var timeProvider = GetTimeProvider();
		var sessionId = await CreateSessionAsync(new SessionCreationConfig
		{
			CurrentTime =  timeProvider.GetUtcNow(),
			MentorId = mentorUser.UserId
		});
		var studentUser = GetFakeUser();
		await using var setup = BuildBookSetup(sessionId, studentUser);

		// Act
		var result = await setup.Handler.Handle(setup.Command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue(result.ErrorInfo?.Message);

		var session = await GetSessionAsync(sessionId);
		session.Should().NotBeNull();
		session!.Status.Should().Be(SessionStatus.Booked);
		session.StudentId.Should().Be(studentUser.UserId!.Value);
	}

	[Fact]
	public async Task Should_ReturnUnauthorized_When_StudentUserIsNull()
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
		var unauthorizedStudent = GetUnauthorizedUser();
		await using var setup = BuildBookSetup(sessionId, unauthorizedStudent, timeProvider);

		// Act
		var result = await setup.Handler.Handle(setup.Command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorInfo?.Type.Should().Be(ErrorType.Unauthorized);

		var session = await GetSessionAsync(sessionId);
		session.Should().NotBeNull();
		session!.Status.Should().Be(SessionStatus.Free);
		session.StudentId.Should().BeNull();
	}

	[Fact]
	public async Task Should_ReturnNotFound_When_SessionDoesNotExist()
	{
		// Arrange
		var studentUser = GetFakeUser();
		await using var setup = BuildBookSetup(Guid.NewGuid(), studentUser);

		// Act
		var result = await setup.Handler.Handle(setup.Command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorInfo?.Type.Should().Be(ErrorType.NotFound);

		var session = await GetSessionAsync(setup.Command.SessionId);
		session.Should().BeNull();
	}

	[Fact]
	public async Task Should_ReturnConflict_And_NotModifySession_When_SessionIsConcurrentlyBooked()
	{
		var mentorUser = GetFakeUser();
		var timeProvider = GetTimeProvider();
		var sessionId = await CreateSessionAsync(new SessionCreationConfig
		{
			CurrentTime =  timeProvider.GetUtcNow(),
			MentorId = mentorUser.UserId
		});

		var anotherStudent = GetFakeUser();
		var interceptor = new ConcurrentChangeInterceptor(
			async ct => { await BookSessionAsync(sessionId, timeProvider, anotherStudent, ct); });

		var concurrencyPipeline = GetConcurrencyPipeline();
		var studentUser = GetFakeUser();
		await using var setup = BuildBookSetup(
			sessionId,
			studentUser,
			timeProvider: timeProvider,
			contextInterceptors: [interceptor],
			concurrencyPipeline: concurrencyPipeline);

		// Act
		var result = await setup.Handler.Handle(setup.Command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorInfo?.Type.Should().Be(ErrorType.Conflict);

		var session = await GetSessionAsync(sessionId);
		session.Should().NotBeNull();
		session.Status.Should().Be(SessionStatus.Booked);
		session.StudentId.Should().Be(anotherStudent.UserId!.Value);

		interceptor.SavesCount.Should().Be(1);
	}

	[Fact]
	public async Task Should_ReturnConflict_And_NotModifySession_When_SessionIsConcurrentlyCanceled()
	{
		var mentorUser = GetFakeUser();
		var timeProvider = GetTimeProvider();
		var sessionId = await CreateSessionAsync(new SessionCreationConfig
		{
			CurrentTime =  timeProvider.GetUtcNow(),
			MentorId = mentorUser.UserId
		});

		var interceptor = new ConcurrentChangeInterceptor(
			async ct => await CancelSessionAsync(sessionId, timeProvider, ct));

		var concurrencyPipeline = GetConcurrencyPipeline();
		var studentUser = GetFakeUser();
		await using var setup = BuildBookSetup(
			sessionId,
			studentUser,
			timeProvider: timeProvider,
			contextInterceptors: [interceptor],
			concurrencyPipeline: concurrencyPipeline);

		// Act
		var result = await setup.Handler.Handle(setup.Command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorInfo?.Type.Should().Be(ErrorType.Conflict);

		var session = await GetSessionAsync(sessionId);
		session.Should().NotBeNull();
		session.Status.Should().Be(SessionStatus.Canceled);
		session.StudentId.Should().BeNull();

		interceptor.SavesCount.Should().Be(1);
	}

	public static IEnumerable<object[]> ValidConcurrencyConflictCounts =>
		Enumerable.Range(0, Pipelines.DatabaseConcurrency.MaxRetryAttempts + 1)
			.Select(x => new object[] { x });

	[Theory]
	[MemberData(nameof(ValidConcurrencyConflictCounts))]
	public async Task Should_RetryAndSave_When_SimulatedConcurrencyConflictsDoNotExceedLimit(int conflictsCount)
	{
		// Arrange
		var mentorUser = GetFakeUser();
		var timeProvider = GetTimeProvider();
		var sessionId = await CreateSessionAsync(new SessionCreationConfig
		{
			CurrentTime =  timeProvider.GetUtcNow(),
			MentorId = mentorUser.UserId
		});

		var studentUser = GetFakeUser();
		var interceptor = new SillyExceptionInterceptor(conflictsCount);
		var concurrencyPipeline = GetConcurrencyPipeline();
		await using var setup = BuildBookSetup(
			sessionId,
			studentUser,
			contextInterceptors: [interceptor],
			concurrencyPipeline: concurrencyPipeline);

		// Act
		var result = await setup.Handler.Handle(setup.Command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue(result.ErrorInfo?.Message);

		var session = await GetSessionAsync(sessionId);
		session.Should().NotBeNull();
		session!.Status.Should().Be(SessionStatus.Booked);
		session.StudentId.Should().Be(studentUser.UserId!.Value);

		interceptor.SavesCount.Should().Be(conflictsCount + 1);
	}

	[Fact]
	public async Task Should_ReturnFailure_When_SimulatedConcurrencyConflictsExceedRetryLimit()
	{
		// Arrange
		var conflictsCount = Pipelines.DatabaseConcurrency.MaxRetryAttempts + 1;
		var mentorUser = GetFakeUser();
		var timeProvider = GetTimeProvider();
		var sessionId = await CreateSessionAsync(new SessionCreationConfig
		{
			CurrentTime =  timeProvider.GetUtcNow(),
			MentorId = mentorUser.UserId
		});

		var studentUser = GetFakeUser();
		var interceptor = new SillyExceptionInterceptor(conflictsCount);
		var concurrencyPipeline = GetConcurrencyPipeline();
		var logger = new FakeLogger<Book.Handler>();
		await using var setup = BuildBookSetup(
			sessionId,
			studentUser,
			contextInterceptors: [interceptor],
			concurrencyPipeline: concurrencyPipeline,
			logger: logger);

		// Act
		var result = await setup.Handler.Handle(setup.Command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorInfo?.Type.Should().Be(ErrorType.Failure);

		var session = await GetSessionAsync(sessionId);
		session.Should().NotBeNull();
		session!.Status.Should().Be(SessionStatus.Free);
		session.StudentId.Should().BeNull();

		interceptor.SavesCount.Should().Be(conflictsCount);
		logger.Messages.Count.Should().Be(1);
	}


	private OperationSetup<Book.Command, Book.Handler> BuildBookSetup(
		Guid sessionId,
		FakeCurrentUser currentUser,
		TimeProvider? timeProvider = null,
		FakeLogger<Book.Handler>? logger = null,
		IInterceptor[]? contextInterceptors = null,
		ResiliencePipeline? concurrencyPipeline = null)
	{
		timeProvider ??= GetTimeProvider();
		contextInterceptors ??= [];
		concurrencyPipeline ??= GetEmptyPipeline();

		ILogger<Book.Handler> abstractLogger =
			logger != null
				? logger
				: NullLogger<Book.Handler>.Instance;

		var command = new Book.Command { SessionId = sessionId };
		var setupContext = BuildContext(contextInterceptors);

		var handler = new Book.Handler(
			setupContext,
			currentUser,
			abstractLogger,
			timeProvider,
			concurrencyPipeline);

		return new OperationSetup<Book.Command, Book.Handler>
		{
			Command = command,
			Handler = handler,
			Context = setupContext,
			Logger = logger,
			CurrentUser = currentUser,
			TimeProvider = timeProvider,
		};
	}
}
