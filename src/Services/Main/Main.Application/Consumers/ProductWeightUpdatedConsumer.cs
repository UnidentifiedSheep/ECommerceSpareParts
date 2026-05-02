using Application.Common.Interfaces;
using Contracts.Articles;
using Main.Application.Handlers.ProductWeight.GetProductWeight;
using MassTransit;
using ZiggyCreatures.Caching.Fusion;

namespace Main.Application.Consumers;

public class ProductWeightUpdatedConsumer(IFusionCache fusionCache,
    ICachePolicy<GetProductWeightQuery> cachePolicy) : IConsumer<ProductWeightUpdatedEvent>
{
    public async Task Consume(ConsumeContext<ProductWeightUpdatedEvent> context)
    {
        await fusionCache.RemoveAsync(
            key: cachePolicy.GetCacheKey(new GetProductWeightQuery(context.Message.ProductId)),
            token: context.CancellationToken);
    }
}