using Microsoft.EntityFrameworkCore;

namespace Persistence.Extensions;

public static class EfQueryableExtensions
{
    public static IQueryable<T> ConfigureTracking<T>(this IQueryable<T> query, bool track) where T : class
    {
        return track ? query : query.AsNoTracking();
    }

    public static IQueryable<T> ForUpdate<T>(this IQueryable<T> query) where T : class
    {
        return query.TagWith("ForUpdate");
    }
}