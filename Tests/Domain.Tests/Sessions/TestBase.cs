using Domain.Entities;
using Domain.Results;
using FluentAssertions;

namespace Domain.Tests.Sessions;

public abstract class TestBase
{
	protected DateTimeOffset GetCurrentTime()
	{
		return new DateTimeOffset(
			2026, 7, 20,
			10, 0, 0,
			TimeSpan.Zero);
	}

	protected Session CreateSession(DateTimeOffset? currentTime = null, DateTimeOffset? startTime = null)
	{
		return TryCreateNewSession(currentTime: currentTime, startTime: startTime).Value!;
	}

	protected Result<Session> TryCreateNewSession(DateTimeOffset? currentTime = null, Guid? mentorId = null, DateTimeOffset? startTime = null,
		TimeSpan? duration = null)
	{
		currentTime ??= GetCurrentTime();
		mentorId ??= Guid.NewGuid();
		startTime ??= currentTime.Value.AddDays(1);
		duration ??= TimeSpan.FromHours(1);

		return Session.Create(currentTime.Value, mentorId.Value, startTime.Value, duration.Value);
	}

	protected void ShouldBeValidation<T>(Func<Result<T>> arrangeAndActFunc)
	{
		var result = arrangeAndActFunc();
		result.IsSuccess.Should().BeFalse();
		result.ErrorInfo?.Type.Should().Be(Results.ErrorType.Validation);
	}
}
