using Contracts.Products;
using MassTransit;
using Search.Application.Interfaces;
using Search.Application.Interfaces.Product;

namespace Search.Application.Consumers.Product;

public class ProductSizesUpdatedConsumer(
    IIndexSynchronizer<Entities.Product, int> productIndexSynchronizer) : IConsumer<ProductSizesUpdatedEvent>
{
    public Task Consume(ConsumeContext<ProductSizesUpdatedEvent> context)
    {
        return productIndexSynchronizer.Reindex(
            context.Message.ProductId,
            context.CancellationToken);
    }
}
