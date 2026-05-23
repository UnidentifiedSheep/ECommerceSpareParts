using Contracts.StorageContent;
using Main.Application.Interfaces.Cache;
using MassTransit;

namespace Main.Application.Consumers;

public class StorageContentUpdatedConsumer(
    IProductCacheRepository productCache) : IConsumer<StorageContentUpdatedEvent>
{
    public async Task Consume(ConsumeContext<StorageContentUpdatedEvent> context)
    {
        await productCache.InvalidateProductAsync(context.Message.ProductId);
    }
}