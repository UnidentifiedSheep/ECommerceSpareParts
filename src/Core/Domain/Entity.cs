using Domain.Interfaces;

namespace Domain;

public abstract class Entity<TModel, TKey>
    : IEntity<TKey> where TModel : Entity<TModel, TKey> where TKey : notnull
{
    public abstract TKey GetId();

    object IEntity.GetId()
    {
        return GetId();
    }
}