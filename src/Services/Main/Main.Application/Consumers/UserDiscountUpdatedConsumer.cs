using Contracts.User;
using Main.Application.Interfaces.Cache;
using MassTransit;

namespace Main.Application.Consumers;

public class UserDiscountUpdatedConsumer(
    IUserCacheRepository userCache) : IConsumer<UserDiscountUpdatedEvent>
{
    public async Task Consume(ConsumeContext<UserDiscountUpdatedEvent> context)
    {
        await userCache.InvalidateUserDiscountAsync(context.Message.UserId);
    }
}