using Contracts;
using MassTransit;

namespace NotificationService.Consumers;

public class SessionBookedConsumer(
        ILogger<SessionBookedConsumer> logger)
    : IConsumer<SessionBooked>
{
    public Task Consume(ConsumeContext<SessionBooked> context)
    {
        var message = context.Message;
        logger.LogInformation("Session {SessionId} was booked by student {StudentId} at {BookedAt}",
            message.SessionId,
            message.StudentId,
            message.BookedAt);

        return Task.CompletedTask;
    }
}
