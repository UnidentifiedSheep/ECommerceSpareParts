using Core.Abstractions;
using Core.Interfaces.CacheRepositories;
using Main.Abstractions.Models;

namespace Main.Application.RelatedData;

public class ArticleCrossesRelatedData(ICache cache, TimeSpan? ttl = null) : RelatedDataBase<ArticleCross>(cache, ttl)
{
    private readonly ICache _cache = cache;
    private readonly TimeSpan? _ttl = ttl;

    public override string GetRelatedDataKey(string id)
    {
        return $"article-related-data:{id}";
    }

    public override async Task AddRelatedDataAsync(IEnumerable<string> ids, string relatedKey)
    {
        var keys = ids.Select(GetRelatedDataKey).ToHashSet();
        var values = keys.ToList();
        values.Add(relatedKey);
        await _cache.SetAddAsync(keys, values, _ttl);
    }
}