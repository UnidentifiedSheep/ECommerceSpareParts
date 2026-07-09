using Application.Common.Extensions;
using Contracts.Supplier;
using Integrations.Supplier.Models;
using MassTransit;
using MediatR;
using Pricing.Application.Handlers.Pricing;

namespace Pricing.Application.Consumers;

public class SupplierProductsRequestedConsumer(
    ISender sender
    ) : IConsumer<SupplierProductsRequestedEvent>
{
    public async Task Consume(ConsumeContext<SupplierProductsRequestedEvent> context)
    {
        var supplier = context.Message.Supplier;
        var storageName = context.Message.RequestedStorageFor;
        var products = context.Message
            .Products
            .Select(x => x.FromContract())
            .ToList();

        await sender.Send(
            new RefreshOffersCommand(context.Message.OccurredAt, supplier, storageName, products),
            context.CancellationToken);
    }
}