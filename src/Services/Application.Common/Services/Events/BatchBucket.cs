using Application.Common.Interfaces.Events;
using Domain.Interfaces.Events;

namespace Application.Common.Services.Events;

public sealed class BatchBucket<TEvent> : IBatchBucket
    where TEvent : IBatchableDomainEvent
{
    private readonly List<TEvent> _items = [];
    private readonly Dictionary<string, TEvent> _keyedEvents = new();

    public void Add(IBatchableDomainEvent item)
    {
        var typedItem = (TEvent)item;

        if (typedItem is IKeyedDomainEvent ke)
            _keyedEvents[ke.GetKey()] = typedItem;
        else
            _items.Add(typedItem);
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
