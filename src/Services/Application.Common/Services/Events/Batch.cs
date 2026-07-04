using Domain.Interfaces.Events;

namespace Application.Common.Services.Events;

public sealed record Batch<TEvent>(
    IReadOnlyList<TEvent> Items
) : IDomainEvent
    where TEvent : IBatchableDomainEvent;