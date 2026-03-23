using Contracts.Purchase;
using MassTransit;
using MediatR;

namespace Analytics.Application.Consumers;

public class PurchaseDeletedConsumer(IMediator mediator) : IConsumer<PurchaseDeleteEvent>
{
    public Task Consume(ConsumeContext<PurchaseDeleteEvent> context)
    {
        throw new NotImplementedException();
    }
}