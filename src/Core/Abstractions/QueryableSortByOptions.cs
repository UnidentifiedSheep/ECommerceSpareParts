using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Abstractions;

public class QueryableSortByOptions
{
    private const string DefaultKey = "__default__";
    public static readonly QueryableSortByOptions Value = new();

    private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, object>> _mapDictionary = new();

    public char Delimiter { get; private set; } = '_';

    public void SetDelimiter(char delimiter)
    {
        Delimiter = delimiter;
    }

    public char GetDelimiter()
    {
        return Delimiter;
    }

    public QueryableSortByOptions Map<TSource, TKey>(
        string source,
        Expression<Func<TSource, TKey>> keySelector)
    {
        var type = typeof(TSource);
        source = source.ToLowerInvariant();

        var primary = _mapDictionary.GetOrAdd(type, _ => new ConcurrentDictionary<string, object>());

        var objectSelector = Expression.Lambda<Func<TSource, object>>(
            Expression.Convert(keySelector.Body, typeof(object)),
            keySelector.Parameters);

        primary[source] = objectSelector;
        return this;
    }

    public QueryableSortByOptions MapDefault<TSource, TKey>(
        Expression<Func<TSource, TKey>> keySelector)
    {
        var type = typeof(TSource);

        var primary = _mapDictionary.GetOrAdd(type, _ => new ConcurrentDictionary<string, object>());

        var objectSelector = Expression.Lambda<Func<TSource, object>>(
            Expression.Convert(keySelector.Body, typeof(object)),
            keySelector.Parameters);

        if (!primary.TryAdd(DefaultKey, objectSelector))
            throw new ArgumentException($"DEFAULT|{type} already exists");
        return this;
    }

    public Expression<Func<TEntity, object?>> GetMapping<TEntity>(string source)
    {
        source = source.ToLowerInvariant();
        var type = typeof(TEntity);

        if (!_mapDictionary.TryGetValue(type, out var primary))
            throw new ArgumentException($"{type} mapping not exists");

        if (primary.TryGetValue(source, out var value))
            return (Expression<Func<TEntity, object?>>)value;

        if (primary.TryGetValue(DefaultKey, out var defaultValue))
            return (Expression<Func<TEntity, object?>>)defaultValue;

        throw new ArgumentException($"Mapping '{source}' for {type} not exists and no default provided");
    }
}