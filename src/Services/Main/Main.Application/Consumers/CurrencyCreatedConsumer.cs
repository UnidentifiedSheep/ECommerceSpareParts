using Contracts.Currency;
using MassTransit;

namespace Main.Application.Consumers;

public class CurrencyCreatedConsumer() : IConsumer<CurrencyCreatedEvent>
{
    public async Task Consume(ConsumeContext<CurrencyCreatedEvent> context)
    {
    }
}