using MediatR;

namespace Application.Common.Interfaces.Events;

public interface IDomainEventScope
{
    Task PublishImmediatelyAsync<T>(T @event, CancellationToken ct = default) where T : INotification ;
    void Add<T>(T @event) where T : INotification;
    void AddRange<T>(IEnumerable<T> events) where T : INotification;
    IReadOnlyCollection<INotification> Flush();
}