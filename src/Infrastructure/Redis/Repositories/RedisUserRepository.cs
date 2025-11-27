using Core.Interfaces.CacheRepositories;
using StackExchange.Redis;

namespace Redis.Repositories;

public class RedisUserRepository : IRedisUserRepository
{
    private readonly IDatabase _redis;
    private readonly TimeSpan? _ttl;

    public RedisUserRepository(IDatabase redis, TimeSpan? ttl = null)
    {
        _redis = redis;
        _ttl = ttl;
    }

    public async Task<decimal?> GetUserDiscount(Guid userId)
    {
        var key = GetUserDiscountKey(userId);
        var value = await _redis.StringGetAsync(key);
        decimal.TryParse(value.ToString(), Global.Culture, out var result);
        return value.HasValue ? result : null;
    }

    public async Task SetUserDiscount(Guid userId, decimal discount)
    {
        var key = GetUserDiscountKey(userId);
        await _redis.StringSetAsync(key, discount.ToString(Global.Culture));
        await _redis.KeyExpireAsync(key, _ttl);
    }

    private static string GetUserDiscountKey(Guid userId)
    {
        return $"user-discount:{userId}";
    }
}