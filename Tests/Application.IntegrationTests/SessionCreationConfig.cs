namespace Application.IntegrationTests;

public sealed class SessionCreationConfig
{
	public Guid? MentorId { get; init; }
	public DateTimeOffset? CurrentTime { get; init; }
	public DateTimeOffset? StartTime { get; init; }
	public bool MakeCanceled { get; init; }
}
