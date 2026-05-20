using Contracts.Products;
using MassTransit;
using Search.Application.Interfaces;

namespace Search.Application.Consumers;

public class ProductSizesUpdatedConsumer(
    IProductIndexSynchronizer productIndexSynchronizer) : IConsumer<ProductSizesUpdatedEvent>
{
    public Task Consume(ConsumeContext<ProductSizesUpdatedEvent> context)
    {
        return productIndexSynchronizer.Reindex(
            context.Message.ProductId,
            context.CancellationToken);
    }
}
