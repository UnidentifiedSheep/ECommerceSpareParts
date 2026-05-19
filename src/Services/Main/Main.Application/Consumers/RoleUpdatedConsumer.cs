using Contracts.Auth;
using Main.Application.Interfaces.Cache;
using MassTransit;

namespace Main.Application.Consumers;

public class RoleUpdatedConsumer(
    IUserCacheRepository userCache) : IConsumer<RoleUpdatedEvent>
{
    public async Task Consume(ConsumeContext<RoleUpdatedEvent> context)
    {
        await userCache.InvalidateRolesAsync();
    }
}
