using Domain.Entities;
using FluentAssertions;

namespace Domain.Tests.Sessions;

public class BookTests : TestBase
{
	[Fact]
	public void Should_ReturnSuccess_When_AllParametersValid()
	{
		// Arrange
		var studentId = Guid.NewGuid();
		var currentTime = GetCurrentTime();
		var tomorrow = currentTime.AddDays(1);
		var session = CreateSession(currentTime, tomorrow);

		// Act
		var result = session.Book(currentTime, studentId);

		// Assert
		result.IsSuccess.Should().BeTrue();
		session.StudentId.Should().Be(studentId);
		session.Status.Should().Be(SessionStatus.Booked);
	}

	[Fact]
	public void Should_ReturnConflict_When_SessionIsBooked()
	{
		// Arrange
		var currentTime = GetCurrentTime();
		var tomorrow = currentTime.AddDays(1);
		var session = CreateSession(currentTime, tomorrow);
		var firstStudent = Guid.NewGuid();
		session.Book(currentTime, firstStudent);

		// Act
		var secondStudent = Guid.NewGuid();
		var result = session.Book(currentTime, secondStudent);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorInfo?.Type.Should().Be(Results.ErrorType.Conflict);
		session.StudentId.Should().Be(firstStudent);
	}

	[Fact]
	public void Should_ReturnConflict_When_SessionIsCanceled()
	{
		// Arrange
		var currentTime = GetCurrentTime();
		var tomorrow = currentTime.AddDays(1);
		var session = CreateSession(currentTime, tomorrow);
		session.Cancel(currentTime);

		// Act
		var student = Guid.NewGuid();
		var result = session.Book(currentTime, student);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorInfo?.Type.Should().Be(Results.ErrorType.Conflict);
		session.StudentId.Should().NotBe(student);
		session.Status.Should().Be(SessionStatus.Canceled);
	}

	[Fact]
	public void Should_ReturnConflict_When_BookingTimeEqualsStartTime()
	{
		// Arrange
		var currentTime = GetCurrentTime();
		var tomorrow = currentTime.AddDays(1);
		var session = CreateSession(currentTime, startTime: tomorrow);

		// Act
		var student = Guid.NewGuid();
		var result = session.Book(tomorrow, student);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorInfo?.Type.Should().Be(Results.ErrorType.Conflict);
		session.StudentId.Should().NotBe(student);
		session.Status.Should().Be(SessionStatus.Free);
	}

	[Fact]
	public void Should_ReturnConflict_When_BookingTimeAfterStartTime()
	{
		// Arrange
		var currentTime = GetCurrentTime();
		var tomorrow = currentTime.AddDays(1);
		var session = CreateSession(currentTime, tomorrow);

		// Act
		var student = Guid.NewGuid();
		var afterTomorrow = currentTime.AddDays(2);
		var result = session.Book(afterTomorrow, student);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorInfo?.Type.Should().Be(Results.ErrorType.Conflict);
		session.StudentId.Should().NotBe(student);
		session.Status.Should().Be(SessionStatus.Free);
	}

	[Fact]
	public void Should_ReturnValidation_When_StudentIsEmpty()
	{
		// Arrange
		var studentId = Guid.Empty;
		var now = GetCurrentTime();
		var tomorrow = now.AddDays(1);
		var session = CreateSession(now, tomorrow);

		// Act
		var result = session.Book(now, studentId);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorInfo?.Type.Should().Be(Results.ErrorType.Validation);

		session.StudentId.Should().BeNull();
		session.Status.Should().Be(SessionStatus.Free);
	}
}
