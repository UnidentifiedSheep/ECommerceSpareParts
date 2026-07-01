using Analytics.Application.Interfaces.Services.FactSynchronizers;
using Analytics.Entities;
using Contracts.Sale;
using MassTransit;

namespace Analytics.Application.Consumers;

public class SaleDeletedConsumer(IFactSynchronizer<SalesFact, Guid> synchronizer)
    : IConsumer<SaleDeletedEvent>
{
    public async Task Consume(ConsumeContext<SaleDeletedEvent> context)
    {
        await synchronizer.SynchronizeAsync(context.Message.SaleId);
    }
}