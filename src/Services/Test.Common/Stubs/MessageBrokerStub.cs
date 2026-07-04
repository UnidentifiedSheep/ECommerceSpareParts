using MassTransit;

namespace Tests.Stubs;

public class MessageBrokerStub : IPublishEndpoint
{
    private readonly Lock _sync = new();
    private readonly List<object> _publishedMessages = [];

    public IReadOnlyList<object> PublishedMessages
    {
        get
        {
            lock (_sync)
            {
                return _publishedMessages.ToList();
            }
        }
    }

    public IReadOnlyList<T> PublishedMessagesOfType<T>() where T : class
    {
        lock (_sync)
        {
            return _publishedMessages.OfType<T>().ToList();
        }
    }

    public Task Publish<T>(
        T message,
        IPipe<PublishContext<T>> publishPipe,
        CancellationToken cancellationToken = new())
        where T : class
    {
        AddMessage(message);
        return Task.CompletedTask;
    }

    public Task Publish<T>(
        T message,
        IPipe<PublishContext> publishPipe,
        CancellationToken cancellationToken = new())
        where T : class
    {
        AddMessage(message);
        return Task.CompletedTask;
    }

    public Task Publish(object message, CancellationToken cancellationToken = new())
    {
        AddMessage(message);
        return Task.CompletedTask;
    }

    public Task Publish(
        object message,
        IPipe<PublishContext> publishPipe,
        CancellationToken cancellationToken = new())
    {
        AddMessage(message);
        return Task.CompletedTask;
    }

    public Task Publish(
        object message,
        Type messageType,
        CancellationToken cancellationToken = new())
    {
        AddMessage(message);
        return Task.CompletedTask;
    }

    public Task Publish(
        object message,
        Type messageType,
        IPipe<PublishContext> publishPipe,
        CancellationToken cancellationToken = new())
    {
        AddMessage(message);
        return Task.CompletedTask;
    }

    public Task Publish<T>(object values, CancellationToken cancellationToken = new()) where T : class
    {
        AddMessage(values);
        return Task.CompletedTask;
    }

    public Task Publish<T>(
        object values,
        IPipe<PublishContext<T>> publishPipe,
        CancellationToken cancellationToken = new()) where T : class
    {
        AddMessage(values);
        return Task.CompletedTask;
    }

    public Task Publish<T>(
        object values,
        IPipe<PublishContext> publishPipe,
        CancellationToken cancellationToken = new()) where T : class
    {
        AddMessage(values);
        return Task.CompletedTask;
    }

    public ConnectHandle ConnectPublishObserver(IPublishObserver observer) { return null!; }

    Task IPublishEndpoint.Publish<T>(T message, CancellationToken cancellationToken)
    {
        return Publish(message, cancellationToken);
    }

    private void AddMessage(object message)
    {
        lock (_sync)
        {
            _publishedMessages.Add(message);
        }
    }
}
