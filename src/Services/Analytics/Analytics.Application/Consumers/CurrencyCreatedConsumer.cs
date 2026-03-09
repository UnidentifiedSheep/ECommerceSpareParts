using Abstractions.Interfaces.Services;
using Analytics.Application.Handlers.Currencies.CreateCurrency;
using Contracts.Currency;
using MassTransit;
using MediatR;

namespace Analytics.Application.Consumers;

public class CurrencyCreatedConsumer(IMediator mediator) : IConsumer<CurrencyCreatedEvent>
{
    public async Task Consume(ConsumeContext<CurrencyCreatedEvent> context)
    {
        var currency = context.Message.Currency;
        var command = new CreateCurrencyCommand(currency.Id, currency.ToUsdRate);
        await mediator.Send(command);
    }
}