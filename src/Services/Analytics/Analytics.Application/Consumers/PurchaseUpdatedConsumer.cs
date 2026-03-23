using Contracts.Purchase;
using MassTransit;

namespace Analytics.Application.Consumers;

public class PurchaseUpdatedConsumer : IConsumer<PurchaseUpdateEvent>
{
    public Task Consume(ConsumeContext<PurchaseUpdateEvent> context)
    {
        throw new NotImplementedException();
    }
}