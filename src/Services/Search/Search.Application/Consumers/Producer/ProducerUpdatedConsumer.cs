using Contracts.Producer;
using MassTransit;

namespace Search.Application.Consumers.Producer;

public class ProducerUpdatedConsumer : IConsumer<ProducerUpdatedEvent>
{
    public Task Consume(ConsumeContext<ProducerUpdatedEvent> context)
    {
        throw new NotImplementedException();
    }
}