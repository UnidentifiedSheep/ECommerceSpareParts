using Analytics.Application.Handlers.PurchaseFacts.DeletePurchaseFact;
using Contracts.Purchase;
using MassTransit;
using MediatR;

namespace Analytics.Application.Consumers;

public class PurchaseDeletedConsumer(IMediator mediator) : IConsumer<PurchaseDeleteEvent>
{
    public async Task Consume(ConsumeContext<PurchaseDeleteEvent> context)
    {
        var purchase = context.Message.Purchase;
        await mediator.Send(new DeletePurchaseFactCommand(purchase.Id));
    }
}