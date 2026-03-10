using Application.Sessions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests;

public class CreateSessionTests : IntegrationTestBase
{
	[Fact]
	public async Task Handle_Should_CreateSession_And_SaveToDatabase()
	{
		// Arrange
		var sessionDto = new CreateSessionDto()
		{
			MentorId = Guid.NewGuid(),
			StartTime = DateTime.UtcNow.AddDays(1),
			Duration = TimeSpan.FromHours(1),
		};
		var command = new Create.Command() { Session = sessionDto };
		await using var setupContext = BuildContext();
		var handler = new Create.Handler(setupContext);

		// Act
		var result = await handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue(result.ErrorInfo?.Message);
		result.Value.Should().NotBeEmpty();

		var session = await GetSessionAsync(result.Value, false);
		session.Should().NotBeNull();
		session.MentorId.Should().Be(sessionDto.MentorId);
		session.StartTime.Should().BeCloseTo(sessionDto.StartTime, TimeSpan.FromMilliseconds(1));
		session.Duration.Should().Be(sessionDto.Duration);
	}

	[Fact]
	public async Task Handle_Should_NotSave_When_DomainValidationFails()
	{
		// Arrange
		var sessionDto = new CreateSessionDto()
		{
			MentorId = Guid.Empty, // triggers validation error
			StartTime = DateTime.UtcNow.AddDays(1),
			Duration = TimeSpan.FromHours(1),
		};
		var command = new Create.Command() { Session = sessionDto };
		await using var setupContext = BuildContext();
		var handler = new Create.Handler(setupContext);

		// Act
		var result = await handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorInfo?.Type.Should().Be(Domain.Results.ErrorType.Validation);

		await using var checkupContext = BuildContext();
		var invalidSessions = await checkupContext.Sessions
			.Where(x => x.MentorId == sessionDto.MentorId)
			.ToListAsync();

		invalidSessions.Should().BeEmpty();
	}
}
