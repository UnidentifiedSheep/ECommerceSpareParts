using Abstractions;
using Application.Common.Interfaces.Repositories;

namespace Application.Common.Extensions;

public static class SortByExtensions
{
    public static IQueryable<TEntity> SortBy<TEntity>(
        this IQueryable<TEntity> query,
        string? sortParam)
    {
        var sort = Parse(sortParam);
        var map = QueryableSortByOptions.Value.GetMapping<TEntity>(sort.Field);

        return sort.Desc
            ? query.OrderByDescending(map)
            : query.OrderBy(map);
    }

    public static CriteriaBuilder<TEntity> WithSorting<TEntity>(
        this CriteriaBuilder<TEntity> builder,
        string? sortParam) where TEntity : class
    {
        var sort = Parse(sortParam);
        var map = QueryableSortByOptions.Value.GetMapping<TEntity>(sort.Field);

        if (sort.Desc)
            builder.OrderByDesc(map);
        else
            builder.OrderByAsc(map);

        return builder;
    }

    private static SortDefinition Parse(string? sortParam)
    {
        if (string.IsNullOrWhiteSpace(sortParam))
            return new SortDefinition(string.Empty, false);

        var span = sortParam.Trim().ToLowerInvariant();
        var delimiter = QueryableSortByOptions.Value.GetDelimiter();

        var idx = span.IndexOf(delimiter);

        if (idx < 0)
            return new SortDefinition(span, false);

        var field = span[..idx];
        var dir = span[(idx + 1)..];

        return new SortDefinition(field, IsDesc(dir));
    }

    private static bool IsDesc(string? way)
    {
        return string.Equals(way, "desc", StringComparison.OrdinalIgnoreCase);
    }

    private sealed record SortDefinition(string Field, bool Desc);
}