using Application.Common.Interfaces.Cqrs;
using Contracts.Products;
using Main.Application.Handlers.ProductWeight.GetProductWeight;
using MassTransit;
using ZiggyCreatures.Caching.Fusion;

namespace Main.Application.Consumers;

public class ProductWeightUpdatedConsumer(
    IFusionCache fusionCache,
    ICachePolicy<GetProductWeightQuery> cachePolicy
) : IConsumer<ProductWeightUpdatedEvent>
{
    public async Task Consume(ConsumeContext<ProductWeightUpdatedEvent> context)
    {
        await fusionCache.RemoveAsync(
            cachePolicy.GetCacheKey(new GetProductWeightQuery(context.Message.ProductId)),
            token: context.CancellationToken);
    }
}