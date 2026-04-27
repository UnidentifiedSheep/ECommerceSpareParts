using Abstractions.Interfaces.Cache;
using Abstractions.Interfaces.RelatedData;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Contracts.Currency;
using Main.Entities.Currency;
using MassTransit;

namespace Main.Application.Consumers;

public class CurrencyCreatedConsumer(
    ICacheInvalidator<Currency, int> cacheInvalidator) : IConsumer<CurrencyCreatedEvent>
{
    public async Task Consume(ConsumeContext<CurrencyCreatedEvent> context)
    {
        await cacheInvalidator.Invalidate(context.Message.Currency.Id);
    }
}