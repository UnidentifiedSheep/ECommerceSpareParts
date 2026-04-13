using Domain.Interfaces;

namespace Domain;

public abstract class Entity<TModel, TKey> : IEntity<TKey>
{
    public abstract TKey GetId();
    object IEntity.GetId()
    {
        return GetId()!;
    }
}