using Abstractions.Interfaces.Cache;
using Abstractions.Models;
using Pricing.Abstractions.Interfaces.CacheRepositories;

namespace Pricing.Cache.Repositories;

public class UserCacheRepository : IUserCacheRepository
{
    private readonly ICache _cache;
    private readonly TimeSpan? _ttl;

    public UserCacheRepository(ICache cache, TimeSpan? ttl = null)
    {
        _cache = cache;
        _ttl = ttl;
    }
    
    public async Task SetUserDiscount(Guid userId, decimal discount, DateTime? setAt = null)
    {
        setAt ??= DateTime.UtcNow;
        var model = new Timestamped<decimal>
        {
            Value = discount,
            Timestamp = setAt.Value
        };
        await _cache.StringSetAsync(GetUserDiscountKey(userId), model, _ttl);
    }

    public async Task DeleteUserDiscount(Guid userId)
    {
        await _cache.DeleteAsync(GetUserDiscountKey(userId));
    }

    public async Task<Timestamped<decimal>?> GetUserDiscount(Guid userId)
    {
        var result = await _cache.StringGetAsync<Timestamped<decimal>>(GetUserDiscountKey(userId));
        return result; 
    }
    
    private static string GetUserDiscountKey(Guid userId) => $"user-discount:{userId}";
}