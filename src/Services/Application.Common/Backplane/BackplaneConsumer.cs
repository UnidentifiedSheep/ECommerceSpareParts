using MassTransit;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Backplane;

namespace Application.Common.Backplane;

public class BackplaneConsumer(IBackplaneDispatcher dispatcher, IFusionCache cache) : IConsumer<BackplaneMessage>
{
    public async Task Consume(ConsumeContext<BackplaneMessage> context)
    {
        if (dispatcher.Handler is null) return;
        if (context.Message.SourceId == cache.InstanceId) return;
        await dispatcher.Handler(context.Message);
    }
}
