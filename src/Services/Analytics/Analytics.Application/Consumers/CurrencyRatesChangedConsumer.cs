using Abstractions.Interfaces.Services;
using Analytics.Core.Entities;
using Analytics.Core.Interfaces.DbRepositories;
using Contracts.Currency;
using MassTransit;

namespace Analytics.Application.Consumers;

public class CurrencyRatesChangedConsumer(ICurrencyRepository currencyRepository, IUnitOfWork unitOfWork) : IConsumer<CurrencyRateChangedEvent>
{
    public async Task Consume(ConsumeContext<CurrencyRateChangedEvent> context)
    {
        
    }
}