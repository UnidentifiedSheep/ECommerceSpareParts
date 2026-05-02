using Application.Common.Interfaces;
using Contracts.Articles;
using Main.Application.Handlers.Products.GetProductCrosses;
using MassTransit;
using ZiggyCreatures.Caching.Fusion;

namespace Main.Application.Consumers;

public class ProductUpdatedConsumer(
    IFusionCache cache) : IConsumer<ProductUpdatedEvent>
{
    public async Task Consume(ConsumeContext<ProductUpdatedEvent> context)
    {
        await cache.RemoveByTagAsync(
            tags: ["product-crosses"],
            token: context.CancellationToken);
    }
}