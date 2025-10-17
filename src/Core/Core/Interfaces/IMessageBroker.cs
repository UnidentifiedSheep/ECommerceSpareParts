namespace Core.Interfaces;

public interface IMessageBroker
{
    Task Publish<T>(T message, CancellationToken cancellationToken = default);
}