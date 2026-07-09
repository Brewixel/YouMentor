using Domain.Results;

namespace Domain.Entities;

public class Session
{
	public Guid Id { get; private set; }

	public Guid MentorId { get; private set; }
	public Guid? StudentId { get; private set; }

	public DateTimeOffset StartTime { get; private set; }
	public TimeSpan Duration { get; private set; }

	public SessionStatus Status { get; private set; }
	public uint Version { get; private set; }

	private Session(Guid mentorId, DateTimeOffset startTime, TimeSpan duration)
	{
		Id = Guid.NewGuid();
		MentorId = mentorId;
		StartTime = startTime.ToUniversalTime();
		Duration = duration;
		Status = SessionStatus.Free;
	}

	private Session() { }

	public static Result<Session> Create(DateTimeOffset currentTime, Guid mentorId, DateTimeOffset startTime, TimeSpan duration)
	{
		if (startTime <= currentTime)
			return Result<Session>.Validation("The session must start in the future.");

		if (mentorId == Guid.Empty)
			return Result<Session>.Validation("Incorrect mentor Id.");

		if (duration <= TimeSpan.Zero)
			return Result<Session>.Validation("Incorrect duration.");

		return Result<Session>.Success(
			new Session(mentorId, startTime, duration));
	}

	public Result Book(DateTimeOffset currentTime, Guid studentId)
	{
		if (Status != SessionStatus.Free)
			return Result.Conflict("This slot is already taken or cancelled.");

		if (StartTime <= currentTime)
			return Result.Conflict("Session cannot be booked.");

		if (studentId == Guid.Empty)
			return Result.Validation("Incorrect student Id.");

		StudentId = studentId;
		Status = SessionStatus.Booked;

		return Result.Success();
	}

	public Result Cancel(DateTimeOffset currentTime)
	{
		if (Status is not (SessionStatus.Free
							or SessionStatus.Booked))
			return Result.Conflict($"Session with status '{Status}' cannot be cancelled.");

		if (StartTime <= currentTime)
			return Result.Conflict("Session cannot be cancelled after it has started.");

		Status = SessionStatus.Canceled;
		return Result.Success();
	}
}
