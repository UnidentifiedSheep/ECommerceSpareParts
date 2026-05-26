using Contracts.Purchase;
using MassTransit;

namespace Search.Application.Consumers.Purchase;

public class PurchaseUpdatedConsumer : IConsumer<PurchaseUpdateEvent>
{
    public Task Consume(ConsumeContext<PurchaseUpdateEvent> context)
    {
        throw new NotImplementedException();
    }
}