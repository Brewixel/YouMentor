using Contracts;

namespace Application.Interfaces;

public interface INotificationSender
{
	Task SendAsync(SendNotification notification, CancellationToken cancellationToken);
}
