using System.Linq.Expressions;

namespace Abstractions.Models.Repository;

public class PageableQueryOptions<TEntity> : QueryOptions<TEntity>
{
    public int? Page { get; protected set; }
    public int? Size { get; protected set; }
    
    private readonly List<OrderExpression<TEntity>> _orderBy = [];
    public IReadOnlyList<OrderExpression<TEntity>> OrderBy => _orderBy;

    public virtual PageableQueryOptions<TEntity> WithPage(int? page)
    {
        if (page.HasValue) ArgumentOutOfRangeException.ThrowIfNegative(page.Value);
        Page = page;
        return this;
    }

    public virtual PageableQueryOptions<TEntity> WithSize(int? size)
    {
        if (size.HasValue) ArgumentOutOfRangeException.ThrowIfNegative(size.Value);
        Size = size;
        return this;
    }
    
    public PageableQueryOptions<TEntity> WithOrderBy(
        Expression<Func<TEntity, object?>> keySelector)
    {
        _orderBy.Add(new OrderExpression<TEntity>(keySelector, false));
        return this;
    }

    public PageableQueryOptions<TEntity> WithOrderByDescending(
        Expression<Func<TEntity, object?>> keySelector)
    {
        _orderBy.Add(new OrderExpression<TEntity>(keySelector, true));
        return this;
    }
    
    public override PageableQueryOptions<TEntity> WithInclude(Expression<Func<TEntity, object?>> include)
    {
        base.WithInclude(include);
        return this;
    }

    public override PageableQueryOptions<TEntity> WithTracking(bool track = true)
    {
        base.WithTracking(track);
        return this;
    }

    public override PageableQueryOptions<TEntity> WithForUpdate(bool forUpdate = true)
    {
        base.WithForUpdate(forUpdate);
        return this;
    }
}