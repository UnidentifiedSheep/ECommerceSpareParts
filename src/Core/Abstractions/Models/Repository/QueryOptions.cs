using System.Linq.Expressions;

namespace Abstractions.Models.Repository;

/// <summary>
/// Base query options without entity-specific configuration.
/// </summary>
public class QueryOptionsBase<TEntity, TSelf>
    where TSelf : QueryOptionsBase<TEntity, TSelf>
{
    public bool Track { get; private set; }
    public bool ForUpdate { get; private set; }

    protected readonly List<Expression<Func<TEntity, object?>>> _includes = [];
    public IReadOnlyList<Expression<Func<TEntity, object?>>> Includes => _includes;

    protected readonly List<OrderExpression<TEntity>> _orderBy = [];
    public IReadOnlyList<OrderExpression<TEntity>> OrderBy => _orderBy;

    public int? Page { get; private set; }
    public int? Size { get; private set; }

    public TSelf WithTracking(bool track = true)
    {
        Track = track;
        return (TSelf)this;
    }

    public TSelf WithForUpdate(bool forUpdate = true)
    {
        ForUpdate = forUpdate;
        return (TSelf)this;
    }

    public TSelf WithInclude(Expression<Func<TEntity, object?>> include)
    {
        ArgumentNullException.ThrowIfNull(include);
        _includes.Add(include);
        return (TSelf)this;
    }

    public TSelf WithOrderBy(Expression<Func<TEntity, object?>> keySelector)
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        _orderBy.Add(new OrderExpression<TEntity>(keySelector, false));
        return (TSelf)this;
    }

    public TSelf WithOrderByDescending(Expression<Func<TEntity, object?>> keySelector)
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        _orderBy.Add(new OrderExpression<TEntity>(keySelector, true));
        return (TSelf)this;
    }

    public TSelf WithPage(int? page)
    {
        if (page.HasValue)
            ArgumentOutOfRangeException.ThrowIfNegative(page.Value);

        Page = page;
        return (TSelf)this;
    }

    public TSelf WithSize(int? size)
    {
        if (size.HasValue)
            ArgumentOutOfRangeException.ThrowIfNegative(size.Value);

        Size = size;
        return (TSelf)this;
    }
}

public class QueryOptions<TEntity>
    : QueryOptionsBase<TEntity, QueryOptions<TEntity>>
{
}

public class QueryOptions<TEntity, TData>
    : QueryOptionsBase<TEntity, QueryOptions<TEntity, TData>>
{
    public required TData Data { get; init; }
}