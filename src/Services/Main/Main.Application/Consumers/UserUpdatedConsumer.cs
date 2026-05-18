using Contracts.User;
using MassTransit;
using ZiggyCreatures.Caching.Fusion;

namespace Main.Application.Consumers;

public class UserUpdatedConsumer(IFusionCache cache) : IConsumer<UserUpdatedEvent>
{
    public async Task Consume(ConsumeContext<UserUpdatedEvent> context)
    {
        var key = CacheKeys.UserCache.GetUserCacheKey(context.Message.UserId);
        await cache.RemoveByTagAsync([key]);
    }
}