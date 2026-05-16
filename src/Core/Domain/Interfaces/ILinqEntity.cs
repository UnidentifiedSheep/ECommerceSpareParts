using System.Linq.Expressions;

namespace Domain.Interfaces;

public interface ILinqEntity<TModel, TKey> 
    where TModel : Entity<TModel, TKey> 
    where TKey : notnull
{
    public static virtual Expression<Func<TModel, TKey>> GetKeySelector()
        => throw new NotSupportedException($"{typeof(TModel).Name} does not define a key selector.");

    public static abstract Expression<Func<TModel, bool>> GetEqualityExpression(TKey key);
}
