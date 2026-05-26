using Analytics.Application.Interfaces.Services.FactSynchronizers;
using Analytics.Entities;
using Contracts.Purchase;
using MassTransit;

namespace Analytics.Application.Consumers;

public class PurchaseUpdatedConsumer(IFactSynchronizer<PurchasesFact, Guid> synchronizer) : IConsumer<PurchaseUpdateEvent>
{
    public async Task Consume(ConsumeContext<PurchaseUpdateEvent> context)
    {
        await synchronizer.SynchronizeAsync(context.Message.PurchaseId);
    }
}