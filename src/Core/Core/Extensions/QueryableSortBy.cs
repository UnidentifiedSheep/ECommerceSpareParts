using System.Linq.Expressions;
using Exceptions.Exceptions;

namespace Core.Extensions;

public static class QueryableSortBy
{
    private static readonly Dictionary<Type, Dictionary<string, object>> MapDictionary = new();
    private static char Delimiter { get; set; } = '_';

    public static void SetDelimiter(char delimiter)
    {
        Delimiter = delimiter;
    }

    public static char GetDelimiter()
    {
        return Delimiter;
    }

    public static TSource Map<TSource, TKey>(this TSource src, string source,
        Expression<Func<TSource, TKey>> keySelector)
    {
        var type = typeof(TSource);
        source = source.ToLowerInvariant();
        MapDictionary.TryAdd(type, new Dictionary<string, object>());
        var primary = MapDictionary[type];
        var objectKeySelector = Expression.Lambda<Func<TSource, object>>(
            Expression.Convert(keySelector.Body, typeof(object)), keySelector.Parameters);
        var added = primary.TryAdd(source, objectKeySelector);
        if (!added) throw new MappingAlreadyExists($"{source}|{type}");
        return src;
    }

    public static TSource MapDefault<TSource, TKey>(this TSource src, Expression<Func<TSource, TKey>> keySelector)
    {
        var type = typeof(TSource);
        MapDictionary.TryAdd(type, new Dictionary<string, object>());
        var primary = MapDictionary[type];
        var objectKeySelector = Expression.Lambda<Func<TSource, object>>(
            Expression.Convert(keySelector.Body, typeof(object)), keySelector.Parameters);
        var added = primary.TryAdd("DEFAULT", objectKeySelector);
        if (!added) throw new MappingAlreadyExists($"DEFAULT|{type}");
        return src;
    }

    private static Expression<Func<TEntity, object>> GetMapping<TEntity>(string source)
    {
        source = source.ToLowerInvariant();
        var type = typeof(TEntity);
        var primaryExists = MapDictionary.TryGetValue(type, out var primary);
        if (!primaryExists) throw new MappingDoesntExists(type);
        var valueExists = primary!.TryGetValue(source, out var value);
        if (!valueExists) primary.TryGetValue("DEFAULT", out value);
        if (value == null) throw new MappingDoesntExists(type);
        return (Expression<Func<TEntity, object>>)value;
    }

    public static Expression<Func<TEntity, object>>? GetExpression<TEntity>(string source)
    {
        source = source.ToLowerInvariant();
        var type = typeof(TEntity);
        var primaryExists = MapDictionary.TryGetValue(type, out var primary);
        if (!primaryExists) throw new MappingDoesntExists(type);
        primary!.TryGetValue(source, out var value);
        return (Expression<Func<TEntity, object>>?)value;
    }

    public static IOrderedQueryable<TSource> SortBy<TSource>(this IQueryable<TSource> src, string? sortParam)
    {
        if (string.IsNullOrWhiteSpace(sortParam))
        {
            var map = GetMapping<TSource>("DEFAULT");
            return src.SortByAscending(map);
        }

        sortParam = sortParam.ToLowerInvariant();
        var sortParams = sortParam.Split(Delimiter);
        var sortName = sortParams[0];
        var sortWay = sortParams.Length > 1 ? sortParams[1] : "asc";
        var mapping = GetMapping<TSource>(sortName);
        return sortWay == "asc" ? src.SortByAscending(mapping) : src.SortByDescending(mapping);
    }

    private static IOrderedQueryable<TSource> SortByDescending<TSource>(this IQueryable<TSource> src,
        Expression<Func<TSource, object>> mapping)
    {
        return src.OrderByDescending(mapping);
    }

    private static IOrderedQueryable<TSource> SortByAscending<TSource>(this IQueryable<TSource> src,
        Expression<Func<TSource, object>> mapping)
    {
        return src.OrderBy(mapping);
    }
}