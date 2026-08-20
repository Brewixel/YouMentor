using Application.IntegrationTests.Infrastructure;
using Application.IntegrationTests.Interceptors;
using Application.Sessions;
using Domain.Entities;
using Domain.Results;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Polly;

namespace Application.IntegrationTests.Sessions;

public class CancelTests : IntegrationTestBase
{
	[Fact]
	public async Task Should_CancelSession_And_SaveToDatabase()
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
		await using var setup = BuildCancelSetup(sessionId, mentorUser, timeProvider);

		// Act
		var result = await setup.Handler.Handle(setup.Command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue(result.ErrorInfo?.Message);

		var session = await GetSessionAsync(sessionId);
		session.Should().NotBeNull();
		session.Status.Should().Be(SessionStatus.Canceled);
	}

	[Fact]
	public async Task Should_ReturnUnauthorized_When_MentorUserIsNull()
	{
		// Arrange
		var validMentor = GetFakeUser();
		var timeProvider = GetTimeProvider();
		var config = new SessionCreationConfig
		{
			CurrentTime =  timeProvider.GetUtcNow(),
			MentorId = validMentor.UserId
		};
		var sessionId = await CreateSessionAsync(config);
		var unauthorizedMentor = GetUnauthorizedUser();
		await using var setup = BuildCancelSetup(sessionId, unauthorizedMentor, timeProvider);

		// Act
		var result = await setup.Handler.Handle(setup.Command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorInfo?.Type.Should().Be(ErrorType.Unauthorized);

		var session = await GetSessionAsync(sessionId);
		session.Should().NotBeNull();
		session.Status.Should().NotBe(SessionStatus.Canceled);
		session.MentorId.Should().Be(validMentor.UserId!.Value);
	}

	[Fact]
	public async Task Should_ReturnNotFound_When_SessionNotExists()
	{
		// Arrange
		var mentorUser = GetFakeUser();
		var timeProvider = GetTimeProvider();
		var sessionId = Guid.NewGuid();
		await using var setup = BuildCancelSetup(sessionId, mentorUser, timeProvider);

		// Act
		var result = await setup.Handler.Handle(setup.Command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorInfo?.Type.Should().Be(ErrorType.NotFound);
		var session = await GetSessionAsync(sessionId);
		session.Should().BeNull();
	}

	[Fact]
	public async Task Should_ReturnForbidden_When_UserIsNotMentor()
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
		var otherUser = GetFakeUser();
		await using var setup = BuildCancelSetup(sessionId, otherUser, timeProvider);

		// Act
		var result = await setup.Handler.Handle(setup.Command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorInfo?.Type.Should().Be(ErrorType.Forbidden);

		var session = await GetSessionAsync(sessionId);
		session.Should().NotBeNull();
		session.Status.Should().NotBe(SessionStatus.Canceled);
	}

	[Fact]
	public async Task Should_ReturnConflict_And_NotModifySession_When_SessionHasStarted()
	{
		// Arrange
		var mentorUser = GetFakeUser();
		var timeProvider = GetTimeProvider();
		var config = new SessionCreationConfig
		{
			CurrentTime =  timeProvider.GetUtcNow(),
			StartTime = timeProvider.GetUtcNow().AddDays(1),
			MentorId = mentorUser.UserId
		};
		var sessionId = await CreateSessionAsync(config);
		await using var setup = BuildCancelSetup(sessionId, mentorUser, timeProvider);

		// Act
		timeProvider.AdjustTime(config.StartTime.Value.AddDays(1));
		var result = await setup.Handler.Handle(setup.Command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorInfo?.Type.Should().Be(ErrorType.Conflict);

		var session = await GetSessionAsync(sessionId);
		session.Should().NotBeNull();
		session.Status.Should().Be(SessionStatus.Free);
	}

	[Fact]
	public async Task Should_CancelSession_When_SessionIsConcurrentlyBooked()
	{
		var mentorUser = GetFakeUser();
		var timeProvider = GetTimeProvider();
		var sessionId = await CreateSessionAsync(new SessionCreationConfig
		{
			CurrentTime =  timeProvider.GetUtcNow(),
			MentorId = mentorUser.UserId
		});

		var studentUser = GetFakeUser();
		var interceptor = new ConcurrentChangeInterceptor(
			async ct => await BookSessionAsync(sessionId, timeProvider, studentUser, ct));

		var concurrencyPipeline = GetConcurrencyPipeline();
		await using var setup = BuildCancelSetup(
			sessionId,
			mentorUser,
			timeProvider: timeProvider,
			contextInterceptors: [interceptor],
			concurrencyPipeline: concurrencyPipeline);

		// Act
		var result = await setup.Handler.Handle(setup.Command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue(result.ErrorInfo?.Message);

		var session = await GetSessionAsync(sessionId);
		session.Should().NotBeNull();
		session.Status.Should().Be(SessionStatus.Canceled);

		interceptor.SavesCount.Should().Be(2);
	}

	[Fact]
	public async Task Should_ReturnConflict_When_SessionIsConcurrentlyCanceled()
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
		await using var setup = BuildCancelSetup(
			sessionId,
			mentorUser,
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

		interceptor.SavesCount.Should().Be(1);
	}


	private OperationSetup<Cancel.Command, Cancel.Handler> BuildCancelSetup(
		Guid sessionId,
		FakeCurrentUser currentUser,
		TimeProvider? timeProvider = null,
		IInterceptor[]? contextInterceptors = null,
		ResiliencePipeline? concurrencyPipeline = null)
	{
		timeProvider ??= GetTimeProvider();
		contextInterceptors ??= [];
		concurrencyPipeline ??= GetEmptyPipeline();

		var command = new Cancel.Command { SessionId = sessionId };
		var setupContext = BuildContext(contextInterceptors);

		var handler = new Cancel.Handler(
			setupContext,
			currentUser,
			timeProvider,
			NullLogger<Cancel.Handler>.Instance,
			concurrencyPipeline);

		return new OperationSetup<Cancel.Command, Cancel.Handler>
		{
			Command = command,
			Handler = handler,
			Context = setupContext,
			CurrentUser = currentUser,
			TimeProvider = timeProvider,
		};
	}
}
