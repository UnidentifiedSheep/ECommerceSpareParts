using System.Linq.Expressions;
using Main.Core.Interfaces.Validation;
using Main.Core.Models;

namespace Main.Application.Validation;

public class ValidationPlan : IValidationPlan
{
    private readonly List<ExistenceCheck> _checks = [];
    public IReadOnlyList<ExistenceCheck> Build() => _checks;

    public IValidationPlan EnsureExists<TEntity, TKey>(Expression<Func<TEntity, TKey>> keySelector, TKey key, Type? errorType = null)
    {
        _checks.Add(ExistenceCheck.Create(key, keySelector, true, errorType));
        return this;
    }

    public IValidationPlan EnsureExists<TEntity, TKey>(Expression<Func<TEntity, TKey>> keySelector, IEnumerable<TKey> keys, Type? errorType = null)
    {
        _checks.Add(ExistenceCheck.CreateRange(keys, keySelector, true, errorType));
        return this;
    }

    public IValidationPlan EnsureNotExists<TEntity, TKey>(Expression<Func<TEntity, TKey>> keySelector, TKey key, Type? errorType = null)
    {
        _checks.Add(ExistenceCheck.Create(key, keySelector, false, errorType));
        return this;
    }

    public IValidationPlan EnsureNotExists<TEntity, TKey>(Expression<Func<TEntity, TKey>> keySelector, IEnumerable<TKey> keys, Type? errorType = null)
    {
        _checks.Add(ExistenceCheck.CreateRange(keys, keySelector, false, errorType));
        return this;
    }
}