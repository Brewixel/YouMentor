namespace Contracts;

public sealed record SessionBooked(
    Guid SessionId,
    Guid MentorId,
    Guid StudentId,
    DateTimeOffset StartTime,
    DateTimeOffset BookedAt);
