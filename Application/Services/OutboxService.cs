using System.Text.Json;
using Application.Interfaces;
using Contracts;
using Domain.Entities;

namespace Application.Services;

public class OutboxService (IAppDbContext context, TimeProvider timeProvider)
	: IOutboxService
{
	public void AddIntent(SendNotification notification)
	{
		var payload = JsonSerializer.Serialize(notification);
		context.OutboxMessages.Add(new OutboxMessage
		{
			Id = Guid.NewGuid(),
			Payload = payload,
			CreatedAt = timeProvider.GetUtcNow()
		});
	}
}
