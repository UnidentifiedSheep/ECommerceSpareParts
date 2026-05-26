using Contracts.Products;
using MassTransit;
using Search.Application.Interfaces;
using Search.Application.Interfaces.Product;

namespace Search.Application.Consumers.Product;

public class ProductWeightUpdatedConsumer(
    IIndexSynchronizer<Entities.Product, int> productIndexSynchronizer) : IConsumer<ProductWeightUpdatedEvent>
{
    public Task Consume(ConsumeContext<ProductWeightUpdatedEvent> context)
    {
        return productIndexSynchronizer.Reindex(
            context.Message.ProductId,
            context.CancellationToken);
    }
}
