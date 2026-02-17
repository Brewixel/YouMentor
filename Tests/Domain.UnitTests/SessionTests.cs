using Domain.Entities;
using Domain.Results;
using FluentAssertions;

namespace Domain.UnitTests;

public class SessionTests
{
	[Fact]
	public void Create_Should_ReturnSuccess_When_AllParametersValid()
	{
		// Arrange
		var mentorId = Guid.NewGuid();
		var now = DateTime.UtcNow;
		var tomorrow = now.AddDays(1);
		var duration = TimeSpan.FromHours(1);

		// Act
		var result = Session.Create(mentorId, tomorrow, now, duration);

		// Assert
		result.IsSuccess.Should().BeTrue();
		var session = result.Value;
		session.Should().NotBeNull();
		session.MentorId.Should().Be(mentorId);
		session.StartTime.Should().Be(tomorrow);
		session.Duration.Should().Be(duration);
		session.Status.Should().Be(SessionStatus.Free);
		session.StudentId.Should().BeNull();
	}

	[Fact]
	public void Create_Should_ReturnSuccess_When_StartDateEqualsNow()
	{
		// Arrange
		var now = DateTime.UtcNow;

		// Act
		var result = TryCreateNewSession(startDate: now, now: now);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.StartTime.Should().Be(now);
	}

	[Fact]
	public void Create_Should_ReturnValidation_When_StartDateLessThanNow()
	{
		ShouldBeValidation(() =>
		{
			// Arrange
			var now = DateTime.UtcNow;
			var yesterday = now.AddDays(-1);

			// Act
			return TryCreateNewSession(startDate: yesterday, now: now);
		});
	}

	[Fact]
	public void Create_Should_ReturnValidation_When_MentorIsEmpty()
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
	public void Create_Should_ReturnValidation_When_DurationLessThanZero()
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
	public void Create_Should_ReturnValidation_When_DurationEqualsZero()
	{
		ShouldBeValidation(() =>
		{
			// Arrange
			var duration = TimeSpan.Zero;

			// Act
			return TryCreateNewSession(duration: duration);
		});
	}

	[Fact]
	public void Book_Should_ReturnSuccess_When_SessionIsFreeAndStudentIsValid()
	{
		// Arrange
		var session = CreateNewSession();
		var studentId = Guid.NewGuid();

		// Act
		var result = session.Book(studentId);

		// Assert
		result.IsSuccess.Should().BeTrue();
		session.StudentId.Should().Be(studentId);
		session.Status.Should().Be(SessionStatus.Booked);
	}

	[Fact]
	public void Book_Should_ReturnValidation_When_StudentIsEmpty()
	{
		// Arrange
		var session = CreateNewSession();
		var studentId = Guid.Empty;

		// Act
		var result = session.Book(studentId);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorInfo?.Type.Should().Be(Results.ErrorType.Validation);

		session.StudentId.Should().BeNull();
		session.Status.Should().Be(SessionStatus.Free);
	}

	[Fact]
	public void Book_Should_ReturnConflict_When_SessionIsBooked()
	{
		// Arrange
		var session = CreateNewSession();
		var firstStudent = Guid.NewGuid();
		session.Book(firstStudent);

		// Act
		var secondStudent = Guid.NewGuid();
		var result = session.Book(secondStudent);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorInfo?.Type.Should().Be(Results.ErrorType.Conflict);
		session.StudentId.Should().Be(firstStudent);
	}

	private Session CreateNewSession()
	{
		return TryCreateNewSession().Value!;
	}

	private Result<Session> TryCreateNewSession(Guid? mentorId = null, DateTime? startDate = null,
		DateTime? now = null, TimeSpan? duration = null)
	{
		mentorId ??= Guid.NewGuid();
		now ??= DateTime.UtcNow;
		startDate ??= now.Value.AddDays(1);
		duration ??= TimeSpan.FromHours(1);

		return Session.Create(mentorId.Value, startDate.Value, now.Value, duration.Value);
	}

	private void ShouldBeValidation<T>(Func<Result<T>> arrangeAndActFunc)
	{
		var result = arrangeAndActFunc();
		result.IsSuccess.Should().BeFalse();
		result.ErrorInfo?.Type.Should().Be(Results.ErrorType.Validation);
	}
}
