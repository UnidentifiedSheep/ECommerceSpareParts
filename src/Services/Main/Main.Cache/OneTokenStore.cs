using Application.Common.Interfaces.Cache;
using Main.Application.Interfaces.Cache;
using Main.Application.Static;

namespace Main.Cache;

public class OneTokenStore(
    ICache cache) : IOneTokenStore
{
    public Task StoreAsync(
        Guid tokenId,
        TimeSpan ttl)
        => cache.SetAsync(
            CacheKeys.OneTimeTokenCache.OneTimeToken(tokenId),
            tokenId,
            ttl);

    public async Task<bool> ConsumeAsync(Guid tokenId)
    {
        var res = await cache.GetDeleteAsync<Guid?>(CacheKeys.OneTimeTokenCache.OneTimeToken(tokenId));
        return res.HasValue && res.Value == tokenId;
    }
}