using Core.Interfaces;

namespace Tests.Stubs;

public class MessageBrokerStub : IMessageBroker
{
    public Task Publish<T>(T message, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}