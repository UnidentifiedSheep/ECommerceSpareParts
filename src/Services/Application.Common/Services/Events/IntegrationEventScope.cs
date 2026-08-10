using Abstractions.Interfaces.Events;
using Application.Common.Interfaces.Events;
using Application.Common.Models;

namespace Application.Common.Services.Events;

public class IntegrationEventScope : IIntegrationEventScope
{
    private readonly List<IntegrationEventEnvelope> _events = [];
    private readonly Dictionary<string, IntegrationEventEnvelope> _keyedEvents = new();

    public void Add<T>(T @event, string? routingKey = null)
    {
        ArgumentNullException.ThrowIfNull(@event);
        var envelope = new IntegrationEventEnvelope(@event, routingKey);

        if (@event is IKeyedEvent ke)
            _keyedEvents[ke.GetKey()] = envelope;
        else
            _events.Add(envelope);
    }

    public void AddRange<T>(
        IEnumerable<T> events,
        string? routingKey = null)
    {
        foreach (var @event in events)
            Add(@event, routingKey);
    }

    public IReadOnlyCollection<IntegrationEventEnvelope> Flush()
    {
        var result = _keyedEvents.Values
            .Concat(_events)
            .ToList();

        _keyedEvents.Clear();
        _events.Clear();
        return result;
    }
}
