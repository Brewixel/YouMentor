using Application.Sessions;
using Domain.Entities;
using Domain.Results;
using FluentAssertions;

namespace Application.IntegrationTests;

public class CancelSessionTests : IntegrationTestBase
{
	[Fact]
	public async Task Handle_Should_CancelSession_And_SaveToDatabase()
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
		var command = new Cancel.Command { SessionId = sessionId };

		await using var setupContext = BuildContext();
		var handler = new Cancel.Handler(setupContext, mentorUser, timeProvider);

		// Act
		var result = await handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue(result.ErrorInfo?.Message);

		var session = await GetSessionAsync(sessionId);
		session.Should().NotBeNull();
		session.Status.Should().Be(SessionStatus.Canceled);
	}

	[Fact]
	public async Task Handle_Should_ReturnUnauthorized_When_MentorUserIsNull()
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
		await CheckSessionPersisted(sessionId);

		var command = new Cancel.Command { SessionId = sessionId };

		await using var setupContext = BuildContext();
		var unauthorizedMentor = GetUnauthorizedUser();
		var handler = new Cancel.Handler(setupContext, unauthorizedMentor, timeProvider);

		// Act
		var result = await handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorInfo?.Type.Should().Be(ErrorType.Unauthorized);

		var session = await GetSessionAsync(sessionId);
		session.Should().NotBeNull();
		session.Status.Should().NotBe(SessionStatus.Canceled);
		session.MentorId.Should().Be(validMentor.UserId!.Value);
	}

	[Fact]
	public async Task Handle_Should_ReturnNotFound_When_SessionNotExists()
	{
		// Arrange
		var mentorUser = GetFakeUser();
		var timeProvider = GetTimeProvider();
		var config = new SessionCreationConfig
		{
			CurrentTime =  timeProvider.GetUtcNow(),
			MentorId = mentorUser.UserId
		};
		await CreateSessionAsync(config);

		var otherSessionId = Guid.NewGuid();
		var command = new Cancel.Command { SessionId = otherSessionId };

		await using var setupContext = BuildContext();
		var handler = new Cancel.Handler(setupContext, mentorUser, timeProvider);

		// Act
		var result = await handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorInfo?.Type.Should().Be(ErrorType.NotFound);
		var session = await GetSessionAsync(otherSessionId);
		session.Should().BeNull();
	}

	[Fact]
	public async Task Handle_Should_ReturnForbidden_When_UserIsNotMentor()
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
		var command = new Cancel.Command { SessionId = sessionId };

		await using var setupContext = BuildContext();
		var otherUser = GetFakeUser();
		var handler = new Cancel.Handler(setupContext, otherUser, timeProvider);

		// Act
		var result = await handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorInfo?.Type.Should().Be(ErrorType.Forbidden);

		var session = await GetSessionAsync(sessionId);
		session.Should().NotBeNull();
		session.Status.Should().NotBe(SessionStatus.Canceled);
	}

	[Fact]
	public async Task Handle_Should_ReturnConflict_And_NotModifySession_When_SessionHasStarted()
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
		await CheckSessionPersisted(sessionId, startTime: config.StartTime);

		var command = new Cancel.Command { SessionId = sessionId };

		await using var setupContext = BuildContext();
		var handler = new Cancel.Handler(setupContext, mentorUser, timeProvider);

		// Act
		timeProvider.AdjustTime(config.StartTime.Value.AddDays(1));
		var result = await handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorInfo?.Type.Should().Be(ErrorType.Conflict);

		var session = await GetSessionAsync(sessionId);
		session.Should().NotBeNull();
		session.Status.Should().Be(SessionStatus.Free);
	}
}
