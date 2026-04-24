using System.Linq.Expressions;

namespace Application.Common.Interfaces.Repositories;

public sealed class CriteriaBuilder<T> where T : class
{
    private Expression<Func<T, bool>>? _where;
    private readonly List<Expression<Func<T, object?>>> _includes = new();
    private Func<IQueryable<T>, IOrderedQueryable<T>>? _orderBy;
    
    private int? _page;
    private int? _size;

    private bool _track;
    private bool _forUpdate;

    public CriteriaBuilder<T> Where(Expression<Func<T, bool>> predicate)
    {
        _where = predicate;
        return this;
    }

    public CriteriaBuilder<T> Include(Expression<Func<T, object?>> include)
    {
        _includes.Add(include);
        return this;
    }

    public CriteriaBuilder<T> OrderByAsc<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        _orderBy = q => q.OrderBy(keySelector);
        return this;
    }

    public CriteriaBuilder<T> OrderByDesc<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        _orderBy = q => q.OrderByDescending(keySelector);
        return this;
    }

    public CriteriaBuilder<T> Page(int skip)
    {
        _page = skip;
        return this;
    }

    public CriteriaBuilder<T> Size(int take)
    {
        _size = take;
        return this;
    }

    public CriteriaBuilder<T> Track(bool track = true)
    {
        _track = track;
        return this;
    }

    public CriteriaBuilder<T> ForUpdate(bool forUpdate = true)
    {
        _forUpdate = forUpdate;
        return this;
    }
    
    public Criteria<T> Build() => new()
    {
        Where = _where,
        OrderBy = _orderBy,
        Page = _page,
        Size = _size,
        Includes = _includes.ToList(),
        Track = _track,
        ForUpdate = _forUpdate
    };
}