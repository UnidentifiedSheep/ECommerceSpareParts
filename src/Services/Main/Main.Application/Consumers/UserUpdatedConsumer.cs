using Contracts.User;
using Main.Application.Models.Cache;
using MassTransit;
using Microsoft.Extensions.Options;
using ZiggyCreatures.Caching.Fusion;

namespace Main.Application.Consumers;

public class UserUpdatedConsumer(
    IFusionCache cache,
    IOptions<CacheSettings> cacheSettings) : IConsumer<UserUpdatedEvent>
{
    public async Task Consume(ConsumeContext<UserUpdatedEvent> context)
    {
        var key = cacheSettings.Value.User.GetUserCacheKey(context.Message.UserId);
        await cache.RemoveByTagAsync([key]);
    }
}