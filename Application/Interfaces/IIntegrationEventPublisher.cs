namespace Application.Interfaces;

public interface IIntegrationEventPublisher
{
	Task PublishAsync<T>(
		T integrationEvent,
		CancellationToken cancellationToken)
	where T : class;
}
