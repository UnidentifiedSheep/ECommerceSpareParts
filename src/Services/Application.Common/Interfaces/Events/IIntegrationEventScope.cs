namespace Application.Common.Interfaces.Events;

public interface IIntegrationEventScope
{
    void Add<T>(T @event);
    void AddRange<T>(IEnumerable<T> events);
    IReadOnlyCollection<object> Flush();
}