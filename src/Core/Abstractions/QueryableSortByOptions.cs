using System.Linq.Expressions;

namespace Abstractions;

public static class QueryableSortByOptions
{
    private const string Defualt = "DEFUALT";
    private static readonly Dictionary<Type, Dictionary<string, object>> MapDictionary = new();
    public static char Delimiter { get; private set; } = '_';

    public static void SetDelimiter(char delimiter)
    {
        Delimiter = delimiter;
    }

    public static char GetDelimiter()
    {
        return Delimiter;
    }

    public static TSource Map<TSource, TKey>(
        this TSource src,
        string source,
        Expression<Func<TSource, TKey>> keySelector)
    {
        var type = typeof(TSource);
        source = source.ToLowerInvariant();
        MapDictionary.TryAdd(type, new Dictionary<string, object>());
        var primary = MapDictionary[type];
        var objectKeySelector = Expression.Lambda<Func<TSource, object>>(
            Expression.Convert(keySelector.Body, typeof(object)), keySelector.Parameters);
        var added = primary.TryAdd(source, objectKeySelector);
        if (!added) throw new ArgumentException($"{source}|{type} Already Exists");
        return src;
    }

    public static TSource MapDefault<TSource, TKey>(this TSource src, Expression<Func<TSource, TKey>> keySelector)
    {
        var type = typeof(TSource);
        MapDictionary.TryAdd(type, new Dictionary<string, object>());
        var primary = MapDictionary[type];
        var objectKeySelector = Expression.Lambda<Func<TSource, object>>(
            Expression.Convert(keySelector.Body, typeof(object)), keySelector.Parameters);
        var added = primary.TryAdd(Defualt, objectKeySelector);
        if (!added) throw new ArgumentException($"DEFAULT|{type} Already Exists");
        return src;
    }

    public static Expression<Func<TEntity, object?>> GetMapping<TEntity>(string source)
    {
        source = source.ToLowerInvariant();
        var type = typeof(TEntity);
        var primaryExists = MapDictionary.TryGetValue(type, out var primary);
        if (!primaryExists) throw new ArgumentException($"{type} mapping not exists");
        var valueExists = primary!.TryGetValue(source, out var value);
        if (!valueExists) primary.TryGetValue(Defualt, out value);
        if (value == null) throw new ArgumentException($"{type} mapping not exists");
        return (Expression<Func<TEntity, object?>>)value;
    }
}