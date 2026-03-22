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
}