using Contracts.Pricing;
using MassTransit;
using MediatR;
using Pricing.Application.Dtos.Price;
using Pricing.Application.Handlers.Pricing;

namespace Pricing.Application.Consumers;

public class ProductPriceOffersUpdatedConsumer(
    ISender sender
    ) : IConsumer<Batch<ProductPriceOffersUpdatedEvent>>
{
    public Task Consume(ConsumeContext<Batch<ProductPriceOffersUpdatedEvent>> context)
    {
        var recalculationRequests = context.Message
            .Select(x => new PriceRecalculationRequestDto
            {
                ProductId = x.Message.ProductId,
                StorageName = x.Message.StorageName
            })
            .ToList();

        return sender.Send(
            new UpsertPriceRecalculationRequestsCommand(recalculationRequests),
            context.CancellationToken);
    }
}

public class ProductPriceOffersUpdatedDefinition
    : ConsumerDefinition<ProductPriceOffersUpdatedConsumer>
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<ProductPriceOffersUpdatedConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        consumerConfigurator.Options<BatchOptions>(options => options
            .SetMessageLimit(100)
            .SetTimeLimit(TimeSpan.FromSeconds(1))
            .SetConcurrencyLimit(1));
    }
}