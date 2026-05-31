using System.Linq.Expressions;

namespace Application.Common.Interfaces.Repositories;

public sealed class CriteriaBuilder<T> where T : class
{
    private readonly List<Expression<Func<T, object?>>> _includes = new();
    private readonly List<Expression<Func<T, bool>>> _wheres = new();
    private bool _forUpdate;
    private bool _skipLocked;
    private Func<IQueryable<T>, IOrderedQueryable<T>>? _orderBy;

    private int? _page;
    private int? _size;

    private bool _track;

    public CriteriaBuilder<T> Where(Expression<Func<T, bool>> predicate)
    {
        _wheres.Add(predicate);
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

    public CriteriaBuilder<T> ForUpdate(bool forUpdate = true, bool skipLocked = false)
    {
        _forUpdate = forUpdate;
        _skipLocked = skipLocked;
        return this;
    }

    public Criteria<T> Build()
    {
        return new Criteria<T>
        {
            Wheres = _wheres.ToList(),
            OrderBy = _orderBy,
            Page = _page,
            Size = _size,
            Includes = _includes.ToList(),
            Track = _track,
            ForUpdate = _forUpdate,
            SkipLocked = _skipLocked
        };
    }
}