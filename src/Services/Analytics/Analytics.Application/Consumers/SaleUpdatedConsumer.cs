using Analytics.Application.Interfaces.Services.FactSynchronizers;
using Contracts.Sale;
using MassTransit;

namespace Analytics.Application.Consumers;

public class SaleUpdatedConsumer(ISaleFactSynchronizer synchronizer) : IConsumer<SaleUpdatedEvent>
{
    public async Task Consume(ConsumeContext<SaleUpdatedEvent> context)
    {
        await synchronizer.SynchronizeAsync(context.Message, context.CancellationToken);
    }
}