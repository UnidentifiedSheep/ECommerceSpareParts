using Abstractions.Interfaces;
using Abstractions.Interfaces.Events;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Notifications;
using MediatR;

namespace Application.Common.Services.Events;

public class DomainEventScope(
    IPublisher publisher) : IDomainEventScope
{
    private readonly Dictionary<string, INotification> _keyedEvents = new();
    private readonly List<INotification> _events = [];
    
    public Task PublishImmediatelyAsync<T>(
        T @event,
        CancellationToken ct = default) where T : INotification
        => publisher.Publish(@event, ct);

    public void Add<T>(T @event) where T : INotification
    {
        ArgumentNullException.ThrowIfNull(@event);
        if (@event is IKeyedNotification ke)
            _keyedEvents[ke.GetKey()] = @event;
        else
            _events.Add(@event);
    }

    public void AddRange<T>(IEnumerable<T> events) where T : INotification
    {
        foreach (var @event in events) Add(@event);
    }

    public IReadOnlyCollection<INotification> Flush()
    {
        var result = _keyedEvents.Values
            .Concat(_events)
            .ToList();
        
        _keyedEvents.Clear();
        _events.Clear();
        return result;
    }
}