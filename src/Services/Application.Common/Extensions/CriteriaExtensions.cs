using Abstractions;
using Application.Common.Interfaces.Repositories;

namespace Application.Common.Extensions;

public static class CriteriaExtensions
{
    public static CriteriaBuilder<TEntity> WithSorting<TEntity>(
        this CriteriaBuilder<TEntity> builder, 
        string? sortParam) where TEntity : class
    {
        var (field, way) = TryGetSortFieldAndWay(sortParam);

        field ??= string.Empty;
        way ??= "asc";

        var map = QueryableSortByOptions.GetMapping<TEntity>(field);
        if (way == "asc")
            builder.OrderByAsc(map);
        else
            builder.OrderByDesc(map);
        return builder;
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