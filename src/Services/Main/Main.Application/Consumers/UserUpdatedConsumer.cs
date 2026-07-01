using Contracts.User;
using Main.Application.Interfaces.Cache;
using MassTransit;

namespace Main.Application.Consumers;

public class UserUpdatedConsumer(IUserCacheRepository userCache) : IConsumer<UserUpdatedEvent>
{
    public async Task Consume(ConsumeContext<UserUpdatedEvent> context)
    {
        await userCache.InvalidateUserAsync(context.Message.UserId);
    }
}