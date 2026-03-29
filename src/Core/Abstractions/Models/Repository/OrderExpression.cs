using System.Linq.Expressions;

namespace Abstractions.Models.Repository;

public record OrderExpression<TEntity>(Expression<Func<TEntity, object?>> keySelector, bool descending)
{
    public Expression<Func<TEntity, object?>> KeySelector { get; } = keySelector;
    public bool Descending { get; } = descending;
}