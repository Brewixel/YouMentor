namespace Contracts;

public sealed record SendNotification(
    Guid NotificationId,
    string Source,
    Guid RecipientId,
    string Channel,
    string Body,
    DateTimeOffset CreatedAt);
