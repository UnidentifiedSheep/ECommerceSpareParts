namespace Application.Common.Interfaces;

public interface IIntegrationEventScope
{
    void Add<T>(T @event);
    IReadOnlyCollection<object> Flush();
}