using Application.Common.Interfaces.Events;
using Domain.Interfaces.Events;
using MediatR;

namespace Application.Common.Services.Events;

public class DomainEventScope(
    IPublisher publisher) : IDomainEventScope
{
    private readonly Dictionary<string, IDomainEvent> _keyedEvents = new();
    private readonly Dictionary<Type, IBatchBucket> _buckets = [];
    private readonly List<IDomainEvent> _events = [];
    
    public Task PublishImmediatelyAsync<T>(
        T @event,
        CancellationToken ct = default) where T : IDomainEvent
        => publisher.Publish(@event, ct);

    public void Add<T>(T @event) where T : IDomainEvent
    {
        ArgumentNullException.ThrowIfNull(@event);
        switch (@event)
        {
            case IBatchableDomainEvent batchable:
                AddBatchable(batchable);
                break;
            case IKeyedDomainEvent ke:
                _keyedEvents[ke.GetKey()] = @event;
                break;
            default:
                _events.Add(@event);
                break;
        }
    }
    
    
    private void AddBatchable<TEvent>(TEvent @event)
        where TEvent : IBatchableDomainEvent
    {
        var type = @event.GetType();

        if (!_buckets.TryGetValue(type, out var bucket))
        {
            bucket = new BatchBucket<TEvent>();
            _buckets[type] = bucket;
        }

        ((IBatchBucket<TEvent>)bucket).Add(@event);
    }

    public void AddRange<T>(IEnumerable<T> events) where T : IDomainEvent
    {
        foreach (var @event in events) Add(@event);
    }

    public IReadOnlyCollection<IDomainEvent> Flush()
    {
        var result = new List<IDomainEvent>(
            _events.Count + _keyedEvents.Count + _buckets.Count);

        result.AddRange(_events);
        result.AddRange(_keyedEvents.Values);

        foreach (var bucket in _buckets.Values)
            if (bucket.Count > 0) result.Add(bucket.BuildNotification());

        _buckets.Clear();
        _keyedEvents.Clear();
        _events.Clear();

        return result;
    }
}