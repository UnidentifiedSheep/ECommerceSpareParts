using Domain.Interfaces;
using Domain.Interfaces.Events;

namespace Domain.Events;

public sealed record EntityCreatedDomainEvent<TEntity>(
    TEntity Entity) : IBatchableDomainEvent
    where TEntity : IEntity;
