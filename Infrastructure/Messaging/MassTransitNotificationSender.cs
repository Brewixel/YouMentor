using Application.Interfaces;
using Contracts;
using MassTransit;

namespace Infrastructure.Messaging;

public sealed class MassTransitNotificationSender(
	ISendEndpointProvider sendEndpointProvider) : INotificationSender
{
	private static readonly Uri Destination = new("queue:notification-service");

	public async Task SendAsync(
		SendNotification notification,
		CancellationToken cancellationToken)
	{
		var endpoint = await sendEndpointProvider.GetSendEndpoint(Destination);

		await endpoint.Send(notification, context =>
		{
			if (context is RabbitMqSendContext rabbitMqContext)
			{
				rabbitMqContext.Mandatory = true;
			}
		}, cancellationToken);
	}
}
