using Application.Common.Interfaces.Cqrs;
using Contracts.Products;
using Main.Application.Handlers.ProductSizes.GetProductSizes;
using MassTransit;
using ZiggyCreatures.Caching.Fusion;

namespace Main.Application.Consumers;

public class ProductSizesUpdatedConsumer(
    IFusionCache cache,
    ICachePolicy<GetProductSizeQuery> cachePolicy) : IConsumer<ProductSizesUpdatedEvent>
{
    public async Task Consume(ConsumeContext<ProductSizesUpdatedEvent> context)
    {
        var key = cachePolicy.GetCacheKey(new GetProductSizeQuery(context.Message.ProductId));
        await cache.RemoveAsync(
            key,
            token: context.CancellationToken);
    }
}