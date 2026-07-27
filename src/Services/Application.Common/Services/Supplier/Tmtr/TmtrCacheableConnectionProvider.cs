using Abstractions.Interfaces.Services;
using Application.Common.Static;
using Integrations.Supplier.Connections;
using Internal.Integration.Core.Interfaces.Common;
using ZiggyCreatures.Caching.Fusion;

namespace Application.Common.Services.Supplier.Tmtr;

public class TmtrCacheableConnectionProvider(
    IFusionCache cache,
    ICommonClient commonClient,
    ISecretEncryptor secretEncryptor
) : TmtrConnectionProvider(commonClient, secretEncryptor)
{
    public override async Task<ConnectionCheck<TmtrConnection>> CheckConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        return await cache.GetOrSetAsync(
            CacheKeys.SettingsCache.TmtrConnection,
            ct => base.CheckConnectionAsync(ct),
            new FusionCacheEntryOptions(CacheKeys.SettingsCache.Ttl),
            cancellationToken);
    }
}
