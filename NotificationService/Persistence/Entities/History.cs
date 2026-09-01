using Contracts;

namespace NotificationService.Persistence.Entities;

public class History
{
	public Guid Id { get; init; }
	public Guid? MessageId { get; init; }
	public DateTimeOffset CreatedAt { get; init; }
	public DateTimeOffset ReceivedAt { get; init; }
	public Guid NotificationId { get; init; }
	public string? Source { get; init; }
	public Guid RecipientId { get; init; }
	public string? Channel { get; init; }
	public string? Body { get; init; }

	private History() { }

	public History(Guid? messageId, DateTimeOffset receivedAt, SendNotification notification)
	{
		Id = Guid.NewGuid();
		MessageId =  messageId;
		ReceivedAt = receivedAt;
		CreatedAt = notification.CreatedAt;
		NotificationId = notification.NotificationId;
		Source = notification.Source;
		RecipientId = notification.RecipientId;
		Channel = notification.Channel;
		Body = notification.Body;
	}
}
