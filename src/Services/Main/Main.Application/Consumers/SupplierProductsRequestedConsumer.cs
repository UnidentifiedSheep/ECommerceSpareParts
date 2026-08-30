using Contracts.Supplier;
using Main.Application.Handlers.ProductEnrichment;
using MassTransit;
using MediatR;

namespace Main.Application.Consumers;

public class SupplierProductsRequestedConsumer(ISender sender) : IConsumer<SupplierProductsRequestedEvent>
{
	public async Task Consume(ConsumeContext<SupplierProductsRequestedEvent> context)
	{
		await sender.Send(
			new ImportSupplierProductCommand(context.Message.Supplier, context.Message.Products),
			context.CancellationToken);
	}
}
