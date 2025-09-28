using Core.Abstractions;
using Core.Entities;
using Core.Interfaces.CacheRepositories;

namespace Application.RelatedData;

public class ArticleRelatedData(ICache cache, TimeSpan? ttl = null) : RelatedDataBase<Article>(cache, ttl)
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