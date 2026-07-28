using Application.Common.Interfaces.Cache;
using Main.Application.Interfaces.Cache;
using Main.Application.Static;

namespace Main.Cache;

public class ResetCache(
    ICache cache) : IResetCache
{
    public Task StoreAsync(
        Guid tokenId,
        TimeSpan ttl)
        => cache.SetAsync(
            CacheKeys.PasswordResetCache.PasswordReset(tokenId),
            tokenId,
            ttl);

    public Task<Guid?> ConsumeAsync(Guid tokenId)
        => cache.GetDeleteAsync<Guid?>(CacheKeys.PasswordResetCache.PasswordReset(tokenId));
}