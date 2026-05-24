using Analytics.Application.Interfaces.Services;
using Contracts.Purchase;
using MassTransit;

namespace Analytics.Application.Consumers;

public class PurchaseDeletedConsumer(IPurchaseFactSynchronizer synchronizer) : IConsumer<PurchaseDeleteEvent>
{
    public async Task Consume(ConsumeContext<PurchaseDeleteEvent> context)
    {
        await synchronizer.SynchronizeAsync(context.Message.PurchaseId);
    }
}