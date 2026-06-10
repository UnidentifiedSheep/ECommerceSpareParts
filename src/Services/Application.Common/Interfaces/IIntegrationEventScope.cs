namespace Application.Common.Interfaces;

public interface IIntegrationEventScope
{
    void Add<T>(T @event);
    void AddRange<T>(IEnumerable<T> events);
    IReadOnlyCollection<object> Flush();
}