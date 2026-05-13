namespace Cache.Extensions;

public readonly record struct CacheArrayResult<T>(
    bool IsHit,
    IReadOnlyList<T> Values);

public static class CacheExtensions
{
    public static async Task<Dictionary<TKey, TValue>> GetOrSetManyAsync<TKey, TValue>(
        this ICache cache,
        IEnumerable<TKey> ids,
        Func<TKey, string> getKey,
        Func<TValue, TKey> getId,
        Func<IReadOnlySet<TKey>, Task<Dictionary<TKey, TValue>>> getMissing,
        TimeSpan? ttl = null)
        where TKey : notnull
    {
        var idsSet = ids.ToHashSet();
        var result = new Dictionary<TKey, TValue>();

        if (idsSet.Count == 0)
            return result;

        var cached = (await cache.GetAsync<TValue>(idsSet.Select(getKey)))
            .DeserializeMany<TValue>();

        foreach (var found in cached)
        {
            if (found == null)
                continue;

            var id = getId(found);
            result[id] = found;
            idsSet.Remove(id);
        }

        if (idsSet.Count == 0)
            return result;

        var missing = await getMissing(idsSet);
        if (missing.Count == 0)
            return result;

        await cache.SetAsync(
            missing.Values.Select(x => (getKey(getId(x)), x)),
            ttl);

        foreach (var item in missing)
            result[item.Key] = item.Value;

        return result;
    }

    public static async Task<CacheArrayResult<T>> GetJsonArrayOrEmptyAsync<T>(
        this ICache cache,
        string key)
    {
        var values = (await cache.GetEnumerableAsync<T>(key))
            .Where(x => x != null)
            .Select(x => x!)
            .ToList();

        if (values.Count != 0)
            return new CacheArrayResult<T>(true, values);

        var exists = await cache.KeyExistsAsync(key);
        return new CacheArrayResult<T>(exists, values);
    }

    public static Task SetJsonArrayAsync<T>(
        this ICache cache,
        string key,
        IEnumerable<T> values,
        TimeSpan? ttl = null)
    {
        return cache.SetEnumerableAsync(
            key: key,
            values: values,
            ttl: ttl);
    }

    public static Task AddRelationsAsync<TKey>(
        this ICache cache,
        IEnumerable<TKey> ids,
        Func<TKey, string> getRelationKey,
        string cacheKey,
        TimeSpan? ttl = null)
    {
        var relations = ids
            .Distinct()
            .GroupBy(getRelationKey)
            .ToDictionary(
                x => x.Key,
                x => x.Select(_ => cacheKey).ToList());

        return cache.AddToSetAsync(relations, ttl);
    }

    public static async Task InvalidateByRelationsAsync(
        this ICache cache,
        string relationKey)
    {
        var keysToDelete = (await cache.GetFromSetAsync(relationKey))
            .ToList();

        if (keysToDelete.Count == 0)
            return;

        await cache.RemoveKeysAsync(keysToDelete);
        await cache.RemoveKeyAsync(relationKey);
    }
}
