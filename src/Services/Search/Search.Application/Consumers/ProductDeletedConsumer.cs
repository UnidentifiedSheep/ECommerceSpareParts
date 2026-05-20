using Contracts.Products;
using MassTransit;
using Search.Application.Interfaces;

namespace Search.Application.Consumers;

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
