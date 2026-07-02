using Abstractions.Interfaces;
using Abstractions.Interfaces.Events;
using Application.Common.Interfaces.Events;

namespace Application.Common.Services.Events;

public class IntegrationEventScope : IIntegrationEventScope
{
    private readonly Dictionary<string, object> _keyedEvents = new();
    private readonly List<object> _events = [];

    public void Add<T>(T @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        if (@event is IKeyedEvent ke)
            _keyedEvents[ke.GetKey()] = @event;
        else
            _events.Add(@event);
    }

    public void AddRange<T>(IEnumerable<T> events)
    {
        foreach (var @event in events) Add(@event);
    }

    public IReadOnlyCollection<object> Flush()
    {
        var result = _keyedEvents.Values
            .Concat(_events)
            .ToList();
        
        _keyedEvents.Clear();
        _events.Clear();
        return result;
    }
}