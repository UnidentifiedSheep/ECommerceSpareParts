namespace Core.Interfaces.MessageBroker;

public interface IEventContext<out T>
{
    T Message { get; }
    Guid MessageId { get; }
    DateTime Timestamp { get; }
}