using Contracts.Producer;
using MassTransit;
using Search.Application.Interfaces;
using Search.Application.Interfaces.Producer;

namespace Search.Application.Consumers.Producer;

public class ProducerUpdatedConsumer(
    IProducerIndexSynchronizer producerIndexSynchronizer) : IConsumer<ProducerUpdatedEvent>
{
    public Task Consume(ConsumeContext<ProducerUpdatedEvent> context)
    {
        return producerIndexSynchronizer.Reindex(
            context.Message.Id,
            context.CancellationToken);
    }
}
