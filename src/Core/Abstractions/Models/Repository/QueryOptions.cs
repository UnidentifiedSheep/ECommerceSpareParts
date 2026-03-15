using System.Linq.Expressions;

namespace Abstractions.Models.Repository;

/// <summary>
/// Used for repository methods configuration. 
/// </summary>
public class QueryOptions
{
    /// <summary>
    /// Indicates that ef should track entities.
    /// <c>False</c> by default
    /// </summary>
    public bool Track { get; protected set; }
    /// <summary>
    /// Indicates that taken entities must be locked.
    /// <c>False</c> by default
    /// </summary>
    public bool ForUpdate { get; protected set; }

    public virtual QueryOptions WithTracking(bool track = true)
    {
        Track = track;
        return this;
    }

    public virtual QueryOptions WithForUpdate(bool forUpdate = true)
    {
        ForUpdate = forUpdate;
        return this;
    }
}

public class QueryOptions<TEntity> : QueryOptions
{
    private readonly List<Expression<Func<TEntity, object?>>> _includes = [];
    public IReadOnlyList<Expression<Func<TEntity, object?>>> Includes => _includes;

    public QueryOptions<TEntity> WithInclude(Expression<Func<TEntity, object?>> include)
    {
        _includes.Add(include);
        return this;
    }

    public override QueryOptions<TEntity> WithTracking(bool track = true)
    {
        base.WithTracking(track);
        return this;
    }

    public override QueryOptions<TEntity> WithForUpdate(bool forUpdate = true)
    {
        base.WithForUpdate(forUpdate);
        return this;
    }
}