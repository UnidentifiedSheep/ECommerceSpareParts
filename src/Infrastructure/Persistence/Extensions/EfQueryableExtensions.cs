using Abstractions.Models.Repository;
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

    public static IQueryable<T> ApplyOptions<T>(this IQueryable<T> query, QueryOptions? config) where T : class
    {
        if (config == null) return query;

        query = query
            .ConfigureTracking(config.Track)
            .ForUpdate(config.ForUpdate);

        if (config is not QueryOptions<T> cfg) return query;
        foreach (var include in cfg.Includes)
            query = query.Include(include);

        return query;
    }

    public static IQueryable<T> ApplyPaging<T>(
        this IQueryable<T> query,
        PageableQueryOptions<T>? options) where T : class
    {
        if (options == null) return query;
        query = query.ApplyOrdering(options);
        
        var page = options.Page;
        var size = options.Size;
        
        if (!size.HasValue) return query;
        
        if (page.HasValue)
            query = query.Skip((page.Value - 1) * size.Value);

        query = query.Take(size.Value);

        return query;
        
    }

    private static IQueryable<T> ApplyOrdering<T>(
        this IQueryable<T> query,
        PageableQueryOptions<T> options) where T : class
    {
        if (!options.OrderBy.Any())
            return query;

        IOrderedQueryable<T>? orderedQuery = null;

        for (int i = 0; i < options.OrderBy.Count; i++)
        {
            var order = options.OrderBy[i];

            if (i == 0)
            {
                orderedQuery = order.Descending
                    ? query.OrderByDescending(order.KeySelector)
                    : query.OrderBy(order.KeySelector);
            }
            else
            {
                orderedQuery = order.Descending
                    ? orderedQuery!.ThenByDescending(order.KeySelector)
                    : orderedQuery!.ThenBy(order.KeySelector);
            }
        }

        return orderedQuery!;
    }
}