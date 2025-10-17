using Core.Interfaces;
using MassTransit;

namespace RabbitMq;

public class MessageBroker(IPublishEndpoint endpoint) : IMessageBroker
{
    public async Task Publish<T>(T message, CancellationToken cancellationToken = default)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        await endpoint.Publish(message, cancellationToken);
    }
}