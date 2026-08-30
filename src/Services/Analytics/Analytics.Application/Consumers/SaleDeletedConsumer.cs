using Analytics.Application.Interfaces.Services.FactSynchronizers;
using Contracts.Sale;
using MassTransit;

namespace Analytics.Application.Consumers;

public class SaleDeletedConsumer(ISaleFactSynchronizer synchronizer) : IConsumer<SaleDeletedEvent>
{
	public async Task Consume(ConsumeContext<SaleDeletedEvent> context)
	{
		await synchronizer.SynchronizeAsync(context.Message, context.CancellationToken);
	}
}
