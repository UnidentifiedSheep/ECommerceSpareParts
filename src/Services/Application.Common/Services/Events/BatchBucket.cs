using Application.Common.Interfaces.Events;
using Domain.Interfaces.Events;

namespace Application.Common.Services.Events;

public sealed class BatchBucket<TEvent> : IBatchBucket<TEvent>
    where TEvent : IBatchableDomainEvent
{
    private readonly List<TEvent> _items = [];
    private readonly Dictionary<string, TEvent> _keyedEvents = new();

    public void Add(TEvent item)
    {
        if (item is IKeyedDomainEvent ke)
            _keyedEvents[ke.GetKey()] = item;
        else
            _items.Add(item);
    }

    public int Count => _items.Count + _keyedEvents.Count;

    public IDomainEvent BuildNotification()
    {
        var result = new List<TEvent>(_items.Count + _keyedEvents.Count);

        result.AddRange(_items);
        result.AddRange(_keyedEvents.Values);

        return new Batch<TEvent>(result);
    }
}