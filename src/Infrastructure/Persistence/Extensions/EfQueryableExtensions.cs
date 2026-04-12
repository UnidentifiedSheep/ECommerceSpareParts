using Application.Common.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Extensions;

public static class EfQueryableExtensions
{
    public static IQueryable<T> ConfigureTracking<T>(this IQueryable<T> query, bool track) where T : class
    {
        return track ? query : query.AsNoTracking();
    }

    public static IQueryable<T> ForUpdate<T>(this IQueryable<T> query, bool forUpdate = true) where T : class
    {
        return forUpdate ? query.TagWith("ForUpdate") : query;
    }

    public static IQueryable<T> Apply<T>(this IQueryable<T> query, Criteria<T> criteria) where T : class
    {
        query.ConfigureTracking(criteria.Track);
        query.ForUpdate(criteria.ForUpdate);
        
        if (criteria.Where is not null) 
            query = query.Where(criteria.Where);
        
        foreach (var i in criteria.Includes) 
            query = query.Include(i);
        
        if (criteria.OrderBy is not null) 
            query = criteria.OrderBy(query);
        
        if (!criteria.Size.HasValue) return query;
        
        if (criteria.Page.HasValue)
            query = query.Skip(criteria.Page.Value * criteria.Size.Value);

        query = query.Take(criteria.Size.Value);
        return query;
    }
}