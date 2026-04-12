using Abstractions;
namespace Extensions;

public static class QueryableExtensions
{
    public static IQueryable<TEntity> WithSorting<TEntity>(
        this IQueryable<TEntity> queryable, 
        string? sortParam)
    {
        var (field, way) = TryGetSortFieldAndWay(sortParam);

        field ??= string.Empty;
        way ??= "asc";

        var map = QueryableSortByOptions.GetMapping<TEntity>(field);

        return way == "asc" ? queryable.OrderBy(map) : queryable.OrderByDescending(map);
    }

    private static (string? Field, string? Direction) TryGetSortFieldAndWay(string? sortParam)
    {
        if (string.IsNullOrWhiteSpace(sortParam))
            return (null, null);

        var parts = sortParam
            .Trim()
            .ToLowerInvariant()
            .Split(QueryableSortByOptions.GetDelimiter(), 2, StringSplitOptions.RemoveEmptyEntries);

        return parts.Length switch
        {
            0 => (null, null),
            1 => (parts[0], null),
            _ => (parts[0], parts[1])
        };
    }
}