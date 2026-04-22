using Abstractions.Interfaces;
using Application.Common.Interfaces;

namespace Application.Common.Services;

public class IntegrationEventScope : IIntegrationEventScope
{
    private readonly Dictionary<string, object> _events = new();

    public void Add<T>(T @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        var key = @event is IKeyedEvent ke
            ? ke.GetKey()
            : Guid.NewGuid().ToString();
        _events[key] = @event;
    }

    public IReadOnlyCollection<object> Flush()
    {
        var result = _events.Values.ToList();
        _events.Clear();
        return result;
    }
}