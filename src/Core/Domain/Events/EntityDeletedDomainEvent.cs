using Domain.Interfaces;
using Domain.Interfaces.Events;

namespace Domain.Events;

public sealed record EntityDeletedDomainEvent<TEntity, TKey>(
    TKey Id) : IBatchableDomainEvent, IKeyedDomainEvent
    where TEntity : IEntity<TKey>
    where TKey : notnull
{
    public string GetKey() => $"{typeof(TEntity).FullName}:{Id}:deleted";
}
