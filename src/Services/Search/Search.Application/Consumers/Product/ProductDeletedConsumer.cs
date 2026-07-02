using Contracts.Products;
using MassTransit;
using Search.Application.Interfaces;

namespace Search.Application.Consumers.Product;

public class ProductDeletedConsumer(
    IIndexSynchronizer<Entities.Product, int> productIndexSynchronizer
) : IConsumer<Batch<ProductDeletedEvent>>
{
    public Task Consume(ConsumeContext<Batch<ProductDeletedEvent>> context)
    {
        return productIndexSynchronizer.Delete(
            context.Message.Select(x => x.Message.Id),
            context.CancellationToken);
    }
}

public class ProductDeletedConsumerDefinition 
    : ConsumerDefinition<ProductDeletedConsumer>
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<ProductDeletedConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        consumerConfigurator.Options<BatchOptions>(options => options
            .SetMessageLimit(100)
            .SetTimeLimit(TimeSpan.FromSeconds(1))
            .SetConcurrencyLimit(1));
    }
}