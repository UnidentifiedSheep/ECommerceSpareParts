using Analytics.Application.Interfaces.Services;
using Contracts.Purchase;
using MassTransit;

namespace Analytics.Application.Consumers;

public class PurchaseUpdatedConsumer(IPurchaseFactSynchronizer synchronizer) : IConsumer<PurchaseUpdateEvent>
{
    public async Task Consume(ConsumeContext<PurchaseUpdateEvent> context)
    {
        await synchronizer.SynchronizeAsync(context.Message.PurchaseId);
    }
}