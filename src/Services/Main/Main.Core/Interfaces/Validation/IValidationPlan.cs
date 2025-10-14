using System.Linq.Expressions;
using Main.Core.Models;

namespace Main.Core.Interfaces.Validation;

public interface IValidationPlan
{
    IReadOnlyList<ExistenceCheck> Build();
    IValidationPlan EnsureExists<TEntity, TKey>(Expression<Func<TEntity, TKey>> keySelector, TKey key, Type? errorType = null);
    IValidationPlan EnsureExists<TEntity, TKey>(Expression<Func<TEntity, TKey>> keySelector, IEnumerable<TKey> keys, Type? errorType = null);
    IValidationPlan EnsureNotExists<TEntity, TKey>(Expression<Func<TEntity, TKey>> keySelector, TKey key, Type? errorType = null);
    IValidationPlan EnsureNotExists<TEntity, TKey>(Expression<Func<TEntity, TKey>> keySelector, IEnumerable<TKey> keys, Type? errorType = null);
}