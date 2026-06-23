using Internal.Integration.Core.Interfaces;
using Internal.Integration.Core.Interfaces.Main;
using ZiggyCreatures.Caching.Fusion;

namespace Pricing.Application.Services;

public class UserServices(
    IFusionCache cache,
    IMainClient mainClient)
{
    public async Task<decimal> GetUserDiscount(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await cache.GetOrSetAsync(
            $"user:{userId}:discount",
            ct => mainClient.UserNode.GetUserDiscount(userId, ct),
            TimeSpan.FromDays(1),
            cancellationToken);
    }
}