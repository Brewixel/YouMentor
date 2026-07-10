using Domain.Entities;
using FluentAssertions;

namespace Domain.Tests.Sessions;

public class CreateTests : TestBase
{
	[Fact]
	public void Should_ReturnSuccess_When_AllParametersValid()
	{
		// Arrange
		var mentorId = Guid.NewGuid();
		var currentTime = GetCurrentTime();
		var startTime = currentTime.AddDays(1);
		var duration = TimeSpan.FromHours(1);

		// Act
		var result = Session.Create(currentTime, mentorId, startTime, duration);

		// Assert
		result.IsSuccess.Should().BeTrue();
		var session = result.Value;
		session.Should().NotBeNull();
		session.MentorId.Should().Be(mentorId);
		session.StartTime.Should().Be(startTime);
		session.Duration.Should().Be(duration);
		session.Status.Should().Be(SessionStatus.Free);
		session.StudentId.Should().BeNull();
	}

	[Fact]
	public void Should_NormalizeStartTimeToUtc()
	{
		// Arrange
		var mentorId = Guid.NewGuid();
		var currentTime = new DateTimeOffset(
			2026, 7, 20,
			14, 0, 0,
			TimeSpan.Zero);
		var startTime = new DateTimeOffset(
			2026, 7, 21,
			14, 0, 0,
			TimeSpan.FromHours(4));
		var duration = TimeSpan.FromHours(1);

		// Act
		var result = Session.Create(currentTime, mentorId, startTime, duration);

		// Assert
		result.IsSuccess.Should().BeTrue();
		var session = result.Value;
		session.Should().NotBeNull();
		session.StartTime.Should().Be(startTime.ToUniversalTime());
		session.StartTime.Offset.Should().Be(TimeSpan.Zero);
	}

	[Fact]
	public void Should_ReturnValidation_When_CreationTimeEqualsStartTime()
	{
		ShouldBeValidation(() =>
		{
			// Arrange
			var currentTime = GetCurrentTime();

			// Act
			return TryCreateNewSession(currentTime: currentTime, startTime: currentTime);
		});
	}

	[Fact]
	public void Should_ReturnValidation_When_CreationTimeAfterStartTime()
	{
		ShouldBeValidation(() =>
		{
			// Arrange
			var currentTime = GetCurrentTime();
			var yesterday = currentTime.AddDays(-1);

			// Act
			return TryCreateNewSession(currentTime: currentTime, startTime: yesterday);
		});
	}

	[Fact]
	public void Should_ReturnValidation_When_MentorIsEmpty()
	{
		ShouldBeValidation(() =>
		{
			// Arrange
			var mentorId = Guid.Empty;

			// Act
			return TryCreateNewSession(mentorId: mentorId);
		});
	}

	[Fact]
	public void Should_ReturnValidation_When_DurationLessThanZero()
	{
		ShouldBeValidation(() =>
		{
			// Arrange
			var duration = TimeSpan.FromHours(-1);

			// Act
			return TryCreateNewSession(duration: duration);
		});
	}

	[Fact]
	public void Should_ReturnValidation_When_DurationEqualsZero()
	{
		ShouldBeValidation(() =>
		{
			// Arrange
			var duration = TimeSpan.Zero;

			// Act
			return TryCreateNewSession(duration: duration);
		});
	}
}
