using System.Linq.Expressions;
using Domain.Interfaces;

namespace Domain;

public abstract class Entity<TModel, TKey>
    : IEntity<TKey> where TModel : Entity<TModel, TKey> where TKey : notnull
{
    public abstract TKey GetId();
    
    public abstract Expression<Func<TModel, bool>> GetEqualityExpression(TKey key);

    object IEntity.GetId()
    {
        return GetId();
    }
}