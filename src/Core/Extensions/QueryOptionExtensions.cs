using Abstractions;
using Abstractions.Models.Repository;

namespace Extensions;

public static class QueryOptionExtensions
{
    public static QueryOptions<TEntity> WithSorting<TEntity>(
        this QueryOptions<TEntity> options,
        string? sortParam)
    {
        return ApplySortingCore<TEntity, QueryOptions<TEntity>>(options, sortParam);
    }
    
    public static QueryOptions<TEntity, TData> WithSorting<TEntity, TData>(
        this QueryOptions<TEntity, TData> options,
        string? sortParam)
    {
        return ApplySortingCore<TEntity, QueryOptions<TEntity, TData>>(options, sortParam);
    }
    
    private static TSelf ApplySortingCore<TEntity, TSelf>(
        TSelf options,
        string? sortParam)
        where TSelf : QueryOptionsBase<TEntity, TSelf>
    {
        var (field, way) = TryGetSortFieldAndWay(sortParam);

        field ??= string.Empty;
        way ??= "asc";

        var map = QueryableSortByOptions.GetMapping<TEntity>(field);

        if (way == "asc")
            options.WithOrderBy(map);
        else
            options.WithOrderByDescending(map);

        return options;
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