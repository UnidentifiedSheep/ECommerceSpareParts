using System.Collections.Concurrent;
using System.Linq.Expressions;
using Abstractions.Models.SortyBy;

namespace Abstractions;

public class QueryableSortBy
{
    private const string DefaultKey = "__default__";
    public static readonly QueryableSortBy Value = new();
    private readonly ConcurrentDictionary<Type, bool> _defaultDirectionMap = new();

    private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, object>> _mapDictionary = new();

    public char Delimiter { get; private set; } = '_';

    public void SetDelimiter(char delimiter) { Delimiter = delimiter; }

    public char GetDelimiter() { return Delimiter; }

    public QueryableSortBy Map<TSource, TKey>(
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

    public QueryableSortBy MapDefault<TSource, TKey>(
        Expression<Func<TSource, TKey>> keySelector,
        bool desc = false)
    {
        var type = typeof(TSource);

        var primary = _mapDictionary.GetOrAdd(type, _ => new ConcurrentDictionary<string, object>());

        var objectSelector = Expression.Lambda<Func<TSource, object>>(
            Expression.Convert(keySelector.Body, typeof(object)),
            keySelector.Parameters);

        primary[DefaultKey] = objectSelector;
        _defaultDirectionMap[type] = desc;
        return this;
    }

    public Expression<Func<TEntity, object?>> GetMapping<TEntity>(string source)
    {
        source = source.ToLowerInvariant();
        var type = typeof(TEntity);

        if (!_mapDictionary.TryGetValue(type, out var primary))
            throw new ArgumentException($"{type} mapping not exists");

        if (primary.TryGetValue(source, out var value)) return (Expression<Func<TEntity, object?>>)value;

        if (primary.TryGetValue(DefaultKey, out var defaultValue))
            return (Expression<Func<TEntity, object?>>)defaultValue;

        throw new ArgumentException($"Mapping '{source}' for {type} not exists and no default provided");
    }

    public bool GetDefaultDesc<TEntity>()
    {
        return _defaultDirectionMap.TryGetValue(typeof(TEntity), out var desc) && desc;
    }

    public static KeySelectorSortDefinition<TEntity> ParseToKeySelector<TEntity>(string? sortParam)
    {
        var sort = ParseToText(sortParam);
        var map = Value.GetMapping<TEntity>(sort.Field);
        var desc = string.IsNullOrEmpty(sort.Field)
            ? Value.GetDefaultDesc<TEntity>()
            : sort.Desc;

        return new KeySelectorSortDefinition<TEntity>(map, desc);
    }

    public static TextSortDefinition ParseToText(string? sortParam)
    {
        if (string.IsNullOrWhiteSpace(sortParam)) return new TextSortDefinition(string.Empty, false);

        var span = sortParam.Trim().ToLowerInvariant();
        var delimiter = Value.GetDelimiter();

        var idx = span.IndexOf(delimiter);

        if (idx < 0) return new TextSortDefinition(span, false);

        var field = span[..idx];
        var dir = span[(idx + 1)..];

        return new TextSortDefinition(field, IsDesc(dir));
    }

    public static bool IsDesc(string? way)
    {
        return string.Equals(
            way,
            "desc",
            StringComparison.OrdinalIgnoreCase);
    }
}