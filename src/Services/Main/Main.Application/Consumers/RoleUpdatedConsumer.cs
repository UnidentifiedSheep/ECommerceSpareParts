using Contracts.Auth;
using MassTransit;
using ZiggyCreatures.Caching.Fusion;

namespace Main.Application.Consumers;

public class RoleUpdatedConsumer(
    IFusionCache cache) : IConsumer<RoleUpdatedEvent>
{
    public async Task Consume(ConsumeContext<RoleUpdatedEvent> context)
    {
        await cache.RemoveByTagAsync("roles");
    }
}