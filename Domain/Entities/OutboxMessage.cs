namespace Domain.Entities;

public class OutboxMessage
{
	public Guid Id { get; set; }
	public string Payload { get; set; } = null!;
	public DateTimeOffset CreatedAt { get; set; }
	public DateTimeOffset? SentAt { get; set; }
}
