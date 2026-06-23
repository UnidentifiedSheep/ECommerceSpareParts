using Analytics.Application.Interfaces.Services.FactSynchronizers;
using Analytics.Entities;
using Contracts.Sale;
using MassTransit;

namespace Analytics.Application.Consumers;

public class SaleUpdatedConsumer(IFactSynchronizer<SalesFact, Guid> synchronizer) : IConsumer<SaleUpdatedEvent>
{
    public async Task Consume(ConsumeContext<SaleUpdatedEvent> context)
    {
        await synchronizer.SynchronizeAsync(context.Message.SaleId);
    }
}
