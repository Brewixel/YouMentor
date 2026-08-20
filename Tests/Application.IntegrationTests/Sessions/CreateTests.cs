using Application.IntegrationTests.Infrastructure;
using Application.Sessions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Application.IntegrationTests.Sessions;

public class CreateTests : IntegrationTestBase
{
	[Fact]
	public async Task Should_CreateSession_And_SaveToDatabase()
	{
		// Arrange
		var mentorUser = GetFakeUser();
		var timeProvider = GetTimeProvider();

		var sessionDto = new CreateSessionDto
		{
			StartTime = timeProvider.GetUtcNow().AddDays(1),
			Duration = TimeSpan.FromHours(1),
		};
		var command = new Create.Command { Session = sessionDto };

		await using var setupContext = BuildContext();
		var handler = new Create.Handler(setupContext, mentorUser, timeProvider);

		// Act
		var result = await handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue(result.ErrorInfo?.Message);
		result.Value.Should().NotBeEmpty();

		var session = await GetSessionAsync(result.Value);
		session.Should().NotBeNull();
		session.MentorId.Should().Be(mentorUser.UserId!.Value);
		session.StartTime.Should().BeCloseTo(sessionDto.StartTime, TimeSpan.FromMilliseconds(1));
		session.Duration.Should().Be(sessionDto.Duration);
	}

	[Fact]
	public async Task Should_ReturnUnauthorized_When_MentorUserIsNull()
	{
		// Arrange
		var unauthorizedMentor = GetUnauthorizedUser();
		var timeProvider = GetTimeProvider();
		var sessionDto = new CreateSessionDto()
		{
			StartTime = timeProvider.GetUtcNow().AddDays(1),
			Duration = TimeSpan.FromHours(1),
		};
		var command = new Create.Command { Session = sessionDto };

		await using var setupContext = BuildContext();
		var handler = new Create.Handler(setupContext, unauthorizedMentor, timeProvider);

		// Act
		var result = await handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorInfo?.Type.Should().Be(Domain.Results.ErrorType.Unauthorized);
	}

	[Fact]
	public async Task Should_NotSave_When_DomainValidationFails()
	{
		// Arrange
		var emptyMentor = GetFakeUser(Guid.Empty); // triggers validation error
		var timeProvider = GetTimeProvider();
		var sessionDto = new CreateSessionDto()
		{
			StartTime = timeProvider.GetUtcNow().AddDays(1),
			Duration = TimeSpan.FromHours(1),
		};
		var command = new Create.Command { Session = sessionDto };

		await using var setupContext = BuildContext();
		var handler = new Create.Handler(setupContext, emptyMentor, timeProvider);

		// Act
		var result = await handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorInfo?.Type.Should().Be(Domain.Results.ErrorType.Validation);

		await using var checkupContext = BuildContext();
		var invalidSessions = await checkupContext.Sessions
			.Where(x => x.MentorId == emptyMentor.UserId)
			.ToListAsync();

		invalidSessions.Should().BeEmpty();
	}
}
