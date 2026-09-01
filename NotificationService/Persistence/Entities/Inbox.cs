using Contracts;

namespace NotificationService.Persistence.Entities;

public class Inbox
{
	public Guid NotificationId { get; init; }
	public string Source { get; init; } = null!;
	public DateTimeOffset ReceivedAt { get; init; }

	private Inbox() { }

	public Inbox (DateTimeOffset receivedAt, SendNotification notification)
	{
		ReceivedAt = receivedAt;
		NotificationId = notification.NotificationId;
		Source = notification.Source;
	}
}
