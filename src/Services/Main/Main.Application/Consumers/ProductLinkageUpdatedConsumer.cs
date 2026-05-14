using Contracts.Products;
using Main.Application.Interfaces.Cache;
using MassTransit;

namespace Main.Application.Consumers;

public class ProductLinkageUpdatedConsumer(
    IProductCacheRepository productCache) : IConsumer<ProductLinkageUpdatedEvent>
{
    public async Task Consume(ConsumeContext<ProductLinkageUpdatedEvent> context)
    {
        await productCache.InvalidateCrossesAsync(context.Message.Id);
    }
}