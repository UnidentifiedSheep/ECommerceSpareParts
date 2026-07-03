using Domain.Interfaces.Events;

namespace Application.Common.Interfaces.Events;

public interface IDomainEventScope
{
    Task PublishImmediatelyAsync<T>(T @event, CancellationToken ct = default) where T : IDomainEvent;
    void Add<T>(T @event) where T : IDomainEvent;
    void AddRange<T>(IEnumerable<T> events) where T : IDomainEvent;
    IReadOnlyCollection<IDomainEvent> Flush();
}