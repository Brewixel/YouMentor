using Contracts;

namespace Application.Interfaces;

public interface IOutboxService
{
	void AddIntent(SendNotification notification);
}
