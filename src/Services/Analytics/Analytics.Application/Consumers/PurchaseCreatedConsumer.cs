using Contracts.Purchase;
using MassTransit;

namespace Analytics.Application.Consumers;

public class PurchaseCreatedConsumer : IConsumer<PurchaseCreatedEvent>
{
    public Task Consume(ConsumeContext<PurchaseCreatedEvent> context)
    {
        //TODO: consume it.
        return Task.CompletedTask;
    }
}