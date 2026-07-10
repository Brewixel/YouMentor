using Domain.Entities;
using FluentAssertions;

namespace Domain.Tests.Sessions;

public class CancelTests : TestBase
{
	[Fact]
	public void Should_ReturnSuccess_When_FreeAndCancellationTimeBeforeStartTime()
	{
		// Arrange
		var currentTime = GetCurrentTime();
		var tomorrow = currentTime.AddDays(1);
		var session = CreateSession(currentTime, tomorrow);

		// Act
		var result = session.Cancel(currentTime);

		// Assert
		result.IsSuccess.Should().BeTrue();
		session.Status.Should().Be(SessionStatus.Canceled);
	}

	[Fact]
	public void Should_ReturnSuccess_When_BookedAndCancellationTimeBeforeStartTime()
	{
		// Arrange
		var currentTime = GetCurrentTime();
		var tomorrow = currentTime.AddDays(1);
		var session = CreateSession(currentTime, tomorrow);
		var studentId = Guid.NewGuid();
		session.Book(currentTime, studentId);
		session.StudentId.Should().Be(studentId);

		// Act
		var result = session.Cancel(currentTime);

		// Assert
		result.IsSuccess.Should().BeTrue();
		session.Status.Should().Be(SessionStatus.Canceled);
		session.StudentId.Should().Be(studentId);
	}

	[Fact]
	public void Should_ReturnConflict_When_AlreadyCancelled()
	{
		// Arrange
		var currentTime = GetCurrentTime();
		var startTime = currentTime.AddDays(1);
		var session = CreateSession(currentTime, startTime);
		session.Cancel(currentTime);
		session.Status.Should().Be(SessionStatus.Canceled);

		// Act
		var result = session.Cancel(currentTime);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorInfo?.Type.Should().Be(Results.ErrorType.Conflict);
		session.Status.Should().Be(SessionStatus.Canceled);
	}

	[Fact]
	public void Should_ReturnConflict_When_CancellationTimeEqualsStartTime()
	{
		// Arrange
		var currentTime = GetCurrentTime();
		var tomorrow = currentTime.AddDays(1);
		var session = CreateSession(currentTime, tomorrow);

		// Act
		var result = session.Cancel(tomorrow);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorInfo?.Type.Should().Be(Results.ErrorType.Conflict);
		session.Status.Should().Be(SessionStatus.Free);
	}

	[Fact]
	public void Should_ReturnConflict_When_CancellationTimeAfterStartTime()
	{
		// Arrange
		var currentTime = GetCurrentTime();
		var tomorrow = currentTime.AddDays(1);
		var session = CreateSession(currentTime, tomorrow);

		// Act
		var afterTomorrow = currentTime.AddDays(2);
		var result = session.Cancel(afterTomorrow);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorInfo?.Type.Should().Be(Results.ErrorType.Conflict);
		session.Status.Should().Be(SessionStatus.Free);
	}
}
