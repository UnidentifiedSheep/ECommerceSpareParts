using Core.Interfaces.MessageBroker;
using MassTransit;

namespace RabbitMq;

public class MassTransitEventContext<TEvent> : IEventContext<TEvent> where TEvent : class
{
    public MassTransitEventContext(ConsumeContext<TEvent> context)
    {
        Message = context.Message;
        MessageId = context.MessageId ?? Guid.Empty;
        Timestamp = DateTime.SpecifyKind(context.SentTime ?? DateTime.UtcNow, DateTimeKind.Utc);
    }

    public TEvent Message { get; }
    public Guid MessageId { get; }
    public DateTime Timestamp { get; }
}