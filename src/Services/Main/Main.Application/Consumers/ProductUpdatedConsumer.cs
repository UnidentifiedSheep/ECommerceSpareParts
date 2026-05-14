using Contracts.Products;
using Main.Application.Interfaces.Cache;
using MassTransit;

namespace Main.Application.Consumers;

public class ProductUpdatedConsumer(
    IProductCacheRepository productCache) : IConsumer<ProductUpdatedEvent>
{
    public async Task Consume(ConsumeContext<ProductUpdatedEvent> context)
    {
        await productCache.InvalidateProductAsync(context.Message.Id);
    }
}
