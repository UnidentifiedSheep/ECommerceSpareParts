using Contracts.Products;
using MassTransit;
using Search.Application.Interfaces;
using Search.Application.Interfaces.Product;

namespace Search.Application.Consumers.Product;

public class ProductUpdatedConsumer(
    IIndexSynchronizer<Entities.Product, int> productIndexSynchronizer) : IConsumer<ProductUpdatedEvent>
{
    public Task Consume(ConsumeContext<ProductUpdatedEvent> context)
    {
        return productIndexSynchronizer.Reindex(
            context.Message.Id,
            context.CancellationToken);
    }
}
