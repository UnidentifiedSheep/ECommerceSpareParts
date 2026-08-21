using System.Linq.Expressions;

namespace Domain.Interfaces;

public interface ILinqEntity<TModel, TKey>
    where TModel : IEntity<TKey>
    where TKey : notnull
{
    public static abstract Expression<Func<TModel, TKey>> GetKeySelector();

    public static abstract Expression<Func<TModel, bool>> GetEqualityExpression(TKey key);
}