using Contracts.Products;
using MassTransit;
using Search.Application.Interfaces;

namespace Search.Application.Consumers;

public class ProductUpdatedConsumer(
    IProductIndexSynchronizer productIndexSynchronizer) : IConsumer<ProductUpdatedEvent>
{
    public Task Consume(ConsumeContext<ProductUpdatedEvent> context)
    {
        return productIndexSynchronizer.Reindex(
            context.Message.Id,
            context.CancellationToken);
    }
}
