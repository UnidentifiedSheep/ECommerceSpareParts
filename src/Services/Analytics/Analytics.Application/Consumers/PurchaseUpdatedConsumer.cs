using Analytics.Abstractions.Dtos.PurchaseFact;
using Analytics.Application.Handlers.PurchaseFacts.UpsertPurchaseFact;
using Contracts.Purchase;
using Mapster;
using MassTransit;
using MediatR;

namespace Analytics.Application.Consumers;

public class PurchaseUpdatedConsumer(IMediator mediator) : IConsumer<PurchaseUpdateEvent>
{
    public async Task Consume(ConsumeContext<PurchaseUpdateEvent> context)
    {
        var data = context.Message.Purchase.Adapt<PurchaseFactUpsertDto>();
        var upsertCommand = new UpsertPurchaseFactCommand(data);
        await mediator.Send(upsertCommand);
    }
}