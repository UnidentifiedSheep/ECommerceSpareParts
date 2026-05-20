using Contracts.Products;
using MassTransit;
using Search.Application.Interfaces;

namespace Search.Application.Consumers;

public class ProductLinkageUpdatedConsumer(
    IProductIndexSynchronizer productIndexSynchronizer) : IConsumer<ProductLinkageUpdatedEvent>
{
    public Task Consume(ConsumeContext<ProductLinkageUpdatedEvent> context)
    {
        return productIndexSynchronizer.Reindex(
            context.Message.Id,
            context.CancellationToken);
    }
}
