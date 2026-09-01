using Application.Interfaces;
using MassTransit;

namespace Infrastructure.Messaging;

public sealed class MassTransitIntegrationEventPublisher(
		IPublishEndpoint publishEndpoint)
	: IIntegrationEventPublisher
{
	public Task PublishAsync<T>(
			T integrationEvent,
			CancellationToken cancellationToken)
		where T : class
	{
		return publishEndpoint.Publish(
			integrationEvent,
			cancellationToken);
	}
}
