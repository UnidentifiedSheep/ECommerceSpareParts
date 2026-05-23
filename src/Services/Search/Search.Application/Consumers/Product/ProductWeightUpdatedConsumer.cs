using Contracts.Products;
using MassTransit;
using Search.Application.Interfaces;

namespace Search.Application.Consumers.Product;

public class ProductWeightUpdatedConsumer(
    IProductIndexSynchronizer productIndexSynchronizer) : IConsumer<ProductWeightUpdatedEvent>
{
    public Task Consume(ConsumeContext<ProductWeightUpdatedEvent> context)
    {
        return productIndexSynchronizer.Reindex(
            context.Message.ProductId,
            context.CancellationToken);
    }
}
