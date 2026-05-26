using Analytics.Application.Interfaces.Services.FactSynchronizers;
using Analytics.Entities;
using Contracts.Purchase;
using MassTransit;

namespace Analytics.Application.Consumers;

public class PurchaseDeletedConsumer(IFactSynchronizer<PurchasesFact, Guid> synchronizer) : IConsumer<PurchaseDeleteEvent>
{
    public async Task Consume(ConsumeContext<PurchaseDeleteEvent> context)
    {
        await synchronizer.SynchronizeAsync(context.Message.PurchaseId);
    }
}