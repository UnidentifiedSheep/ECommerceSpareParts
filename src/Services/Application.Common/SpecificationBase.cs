using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Application.Common;

public abstract class Specification<TEntity, TKey> : ISpecification<TEntity, TKey> where TEntity : Entity<TEntity, TKey>
{
    public bool Track { get; protected set; } = false;
    public bool ForUpdate { get; protected set; } = false;
    
    public int? Page { get; protected set; }
    public int? Size { get; protected set; }
    
    protected readonly List<Expression<Func<TEntity, bool>>> Criteria = [];
    protected readonly List<Expression<Func<TEntity, object?>>> Includes = [];
    protected readonly List<(Expression<Func<TEntity, object>> Key, bool Desc)> OrderBy = [];

    public virtual IQueryable<TEntity> Apply(IQueryable<TEntity> query)
    {
        query.ConfigureTracking(Track);
        query.ForUpdate(ForUpdate);
        
        foreach (var criteria in Criteria)
            query = query.Where(criteria);

        foreach (var include in Includes)
            query = query.Include(include);

        IOrderedQueryable<TEntity>? ordered = null;

        for (int i = 0; i < OrderBy.Count; i++)
        {
            var o = OrderBy[i];

            if (i == 0)
            {
                ordered = o.Desc
                    ? query.OrderByDescending(o.Key)
                    : query.OrderBy(o.Key);
            }
            else
            {
                ordered = o.Desc
                    ? ordered!.ThenByDescending(o.Key)
                    : ordered!.ThenBy(o.Key);
            }
        }

        if (ordered != null)
            query = ordered;

        if (!Size.HasValue) return query;
        
        if (Page.HasValue)
            query = query.Skip(Page.Value * Size.Value);

        query = query.Take(Size.Value);


        return query;
    }
}