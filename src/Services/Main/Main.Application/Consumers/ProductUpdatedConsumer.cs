using Contracts.Products;
using MassTransit;
using ZiggyCreatures.Caching.Fusion;

namespace Main.Application.Consumers;

public class ProductUpdatedConsumer(
    IFusionCache cache) : IConsumer<ProductUpdatedEvent>
{
    public async Task Consume(ConsumeContext<ProductUpdatedEvent> context)
    {
        await cache.RemoveByTagAsync(
            ["product-crosses"],
            token: context.CancellationToken);
    }
}