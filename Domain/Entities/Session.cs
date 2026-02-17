using Domain.Results;

namespace Domain.Entities;

public class Session
{
	public Guid Id { get; private set; }

	public Guid MentorId { get; private set; }
	public Guid? StudentId { get; private set; }

	public DateTime StartTime { get; private set; }
	public TimeSpan Duration { get; private set; }

	public SessionStatus Status { get; private set; }
	public uint Version { get; private set; }

	private Session(Guid mentorId, DateTime startTime, TimeSpan duration)
	{
		Id = Guid.NewGuid();
		MentorId = mentorId;
		StartTime = startTime;
		Duration = duration;
		Status = SessionStatus.Free;
	}

	private Session() { }

	public static Result<Session> Create(Guid mentorId, DateTime startTime, DateTime currentTime, TimeSpan duration)
	{
		if (startTime < currentTime)
			return Result<Session>.Validation("You cannot create a session in the past.");

		if (mentorId == Guid.Empty)
			return Result<Session>.Validation("Incorrect mentor Id.");

		if (duration <= TimeSpan.Zero)
			return Result<Session>.Validation("Incorrect duration.");

		return Result<Session>.Success(
			new Session(mentorId, startTime, duration));
	}

	public Result Book(Guid studentId)
	{
		if (Status != SessionStatus.Free)
			return Result.Conflict("This slot is already taken or cancelled.");

		if (studentId == Guid.Empty)
			return Result.Validation("Incorrect student Id.");

		StudentId = studentId;
		Status = SessionStatus.Booked;

		return Result.Success();
	}
}
