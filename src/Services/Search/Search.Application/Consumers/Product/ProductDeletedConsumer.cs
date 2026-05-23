using Contracts.Products;
using MassTransit;
using Search.Application.Interfaces;
using Search.Application.Interfaces.Product;

namespace Search.Application.Consumers.Product;

public class ProductDeletedConsumer(
    IProductIndexSynchronizer productIndexSynchronizer) : IConsumer<ProductDeletedEvent>
{
    public Task Consume(ConsumeContext<ProductDeletedEvent> context)
    {
        return productIndexSynchronizer.Delete(
            context.Message.Id,
            context.CancellationToken);
    }
}
