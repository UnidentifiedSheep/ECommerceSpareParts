using Contracts.Products;
using MassTransit;
using Search.Application.Interfaces;

namespace Search.Application.Consumers;

public class ProductCreatedConsumer(
    IProductIndexSynchronizer productIndexSynchronizer) : IConsumer<ProductCreatedEvent>
{
    public Task Consume(ConsumeContext<ProductCreatedEvent> context)
    {
        return productIndexSynchronizer.Reindex(
            context.Message.Id,
            context.CancellationToken);
    }
}
