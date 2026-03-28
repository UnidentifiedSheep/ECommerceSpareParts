using Abstractions.Interfaces.Cache;
using Main.Abstractions.Interfaces.CacheRepositories;
using Main.Entities;

namespace Main.Cache.Repositories;

public class UsersCacheRepository : IUsersCacheRepository
{
    private readonly ICache _redis;
    private readonly TimeSpan? _ttl;

    public UsersCacheRepository(ICache redis, TimeSpan? ttl = null)
    {
        _redis = redis;
        _ttl = ttl;
    }

    public async Task<decimal?> GetUserDiscount(Guid userId)
    {
        var key = GetUserDiscountKey(userId);
        var value = await _redis.StringGetAsync(key);
        decimal.TryParse(value, Global.Culture, out var result);
        return !string.IsNullOrWhiteSpace(value) ? result : null;
    }

    public async Task SetUserDiscount(Guid userId, decimal discount)
    {
        var key = GetUserDiscountKey(userId);
        await _redis.StringSetAsync(key, discount.ToString(Global.Culture), _ttl);
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        var key = GetUserByEmailKey(email);
        return await _redis.StringGetAsync<User>(key);
    }

    public async Task SetUserByEmail(string email, User user)
    {
        var key = GetUserByEmailKey(email);
        await _redis.StringSetAsync(key, user, _ttl);
    }

    private static string GetUserByEmailKey(string email)
        => $"user-by-email:{email}";

    private static string GetUserDiscountKey(Guid userId)
    {
        return $"user-discount:{userId}";
    }
}