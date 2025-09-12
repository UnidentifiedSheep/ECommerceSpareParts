using Core.Entities;
using Core.Interfaces.CacheRepositories;
using StackExchange.Redis;

namespace Redis.Repositories;

public class ArticleRelatedDataRepository(IDatabase redis, TimeSpan? ttl = null) : IRelatedDataRepository<Article>
{
    public async Task<IEnumerable<string>> GetRelatedDataKeys(string id)
    {
        var key = GetRelatedDataKey(id);
        var members = await redis.SetMembersAsync(key);
        var ids = members
            .Where(x => x is { IsNullOrEmpty: false })
            .Select(x => x.ToString());
        return ids;
    }

    public async Task AddRelatedDataAsync(string id, string relatedKey)
    {
        var key = GetRelatedDataKey(id);
        await redis.SetAddAsync(key, relatedKey);
        await redis.KeyExpireAsync(key, ttl);
    }

    public async Task AddRelatedDataAsync(string id, IEnumerable<string> relatedKeys)
    {
        var key = GetRelatedDataKey(id);
        var members = relatedKeys.Select(x => new RedisValue(x)).ToArray();
        await redis.SetAddAsync(key, members);
        await redis.KeyExpireAsync(key, ttl);
    }

    private static string GetRelatedDataKey(string id)
    {
        return $"article-related-data:{id}";
    }
}