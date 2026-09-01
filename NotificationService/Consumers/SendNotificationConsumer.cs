using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationService.Persistence;
using NotificationService.Persistence.Entities;
using Npgsql;

namespace NotificationService.Consumers;

public class SendNotificationConsumer(
		AppDbContext dbContext,
		TimeProvider timeProvider,
        ILogger<SendNotificationConsumer> logger)
    : IConsumer<SendNotification>
{
    public async Task Consume(ConsumeContext<SendNotification> context)
    {
        var message = context.Message;
        logger.LogInformation(@"SendNotification command received: NotificationId {NotificationId},
Source: {Source}, RecipientId: {RecipientId}, Channel: {Channel}, MessageId: {MessageId}",
            message.NotificationId,
            message.Source,
            message.RecipientId,
            message.Channel,
            context.MessageId
        );
        var currentTime = timeProvider.GetUtcNow();
        dbContext.Inbox.Add(new Inbox(currentTime, message));
        dbContext.History.Add(new History(context.MessageId, currentTime, message));

        try
        {
	        await dbContext.SaveChangesAsync(context.CancellationToken);
        }
        catch (DbUpdateException exception) when (
	        exception.InnerException is PostgresException
			{
				SqlState: PostgresErrorCodes.UniqueViolation,
				ConstraintName: "PK_Inbox"
			})
        {
	        logger.LogInformation(
		        "Duplicate notification {NotificationId} from {Source} skipped. MessageId: {MessageId}",
		        message.NotificationId,
		        message.Source,
		        context.MessageId);

	        return;
        }
    }
}
