using Main.Modules.Product.Abstractions.Events;
using Main.Modules.Product.Application.Handlers.ProductEnrichment;
using MassTransit;
using MediatR;

namespace Main.Modules.Product.Application.Consumers;

public class SupplierProductsRequestedConsumer(ISender sender) : IConsumer<SupplierProductsRequestedEvent>
{
	public async Task Consume(ConsumeContext<SupplierProductsRequestedEvent> context)
		=> await sender.Send(
			new ImportSupplierProductCommand(context.Message.Supplier, context.Message.Products),
			context.CancellationToken);
}
