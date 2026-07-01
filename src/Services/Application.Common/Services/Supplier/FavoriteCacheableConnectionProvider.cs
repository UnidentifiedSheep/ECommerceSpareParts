using Abstractions.Interfaces.Services;
using Application.Common.Static;
using Integrations.Supplier.Connections;
using Internal.Integration.Core.Interfaces.Common;
using ZiggyCreatures.Caching.Fusion;

namespace Application.Common.Services.Supplier;

public class FavoriteCacheableConnectionProvider(
    IFusionCache cache,
    ICommonClient commonClient,
    ISecretEncryptor secretEncryptor
) : FavoriteConnectionProvider(commonClient, secretEncryptor)
{
    public override async Task<ConnectionCheck<FavoritConnection>> CheckConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        return await cache.GetOrSetAsync(
            CacheKeys.SettingsCache.FavoritSettings,
            ct => base.CheckConnectionAsync(ct),
            new FusionCacheEntryOptions(CacheKeys.SettingsCache.Ttl),
            cancellationToken);
    }
}