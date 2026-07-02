using Contracts.Producer;
using MassTransit;
using Search.Application.Interfaces;

namespace Search.Application.Consumers.Producer;

public class ProducerUpdatedConsumer(
    IIndexSynchronizer<Entities.Producer, int> producerIndexSynchronizer
) : IConsumer<Batch<ProducerUpdatedEvent>>
{
    public Task Consume(ConsumeContext<Batch<ProducerUpdatedEvent>> context)
    {
        return producerIndexSynchronizer.Reindex(
            context.Message.Select(x => x.Message.Id),
            context.CancellationToken);
    }
}

public class ProducerUpdatedConsumerDefinition 
    : ConsumerDefinition<ProducerUpdatedConsumer>
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<ProducerUpdatedConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        consumerConfigurator.Options<BatchOptions>(options => options
            .SetMessageLimit(100)
            .SetTimeLimit(TimeSpan.FromSeconds(1))
            .SetConcurrencyLimit(1));
    }
}