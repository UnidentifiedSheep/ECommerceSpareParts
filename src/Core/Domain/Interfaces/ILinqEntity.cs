using System.Linq.Expressions;

namespace Domain.Interfaces;

public interface ILinqEntity<TModel, TKey> 
    where TModel : Entity<TModel, TKey> 
    where TKey : notnull
{
    public static abstract Expression<Func<TModel, bool>> GetEqualityExpression(TKey key);
}