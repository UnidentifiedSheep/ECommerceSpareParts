using Contracts.Products;
using MassTransit;
using Search.Application.Interfaces;

namespace Search.Application.Consumers.Product;

public class ProductUpdatedConsumer(
    IIndexSynchronizer<Entities.Product, int> productIndexSynchronizer
) : IConsumer<Batch<ProductUpdatedEvent>>
{
    public Task Consume(ConsumeContext<Batch<ProductUpdatedEvent>> context)
    {
        return productIndexSynchronizer.Reindex(
            context.Message.Select(x => x.Message.Id),
            context.CancellationToken);
    }
}

public class ProductUpdatedConsumerDefinition 
    : ConsumerDefinition<ProductUpdatedConsumer>
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<ProductUpdatedConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        consumerConfigurator.Options<BatchOptions>(options => options
            .SetMessageLimit(100)
            .SetTimeLimit(TimeSpan.FromSeconds(1))
            .SetConcurrencyLimit(1));
    }
}