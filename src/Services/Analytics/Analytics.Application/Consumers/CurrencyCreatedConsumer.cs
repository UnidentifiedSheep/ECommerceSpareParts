using Contracts.Currency;
using MassTransit;
using MediatR;

namespace Analytics.Application.Consumers;

public class CurrencyCreatedConsumer(IMediator mediator) : IConsumer<CurrencyCreatedEvent>
{
    public async Task Consume(ConsumeContext<CurrencyCreatedEvent> context)
    {
        //TODO
    }
}