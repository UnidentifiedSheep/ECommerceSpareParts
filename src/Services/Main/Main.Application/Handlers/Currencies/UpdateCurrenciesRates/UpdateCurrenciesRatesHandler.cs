using Abstractions.Interfaces;
using Abstractions.Interfaces.Currency;
using Abstractions.Interfaces.Integrations.ExchangeRate;
using Abstractions.Interfaces.Services;
using Abstractions.Models;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Settings;
using Attributes;
using Contracts.Currency;
using Exceptions.Exceptions.Currencies;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Abstractions.Models.Settings;
using Main.Application.Notifications;
using Main.Entities;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Main.Application.Handlers.Currencies.UpdateCurrenciesRates;

[Transactional]
public record UpdateCurrenciesRatesCommand : ICommand;

public class UpdateCurrenciesRatesHandler(ILogger<UpdateCurrenciesRatesCommand> logger, IExchangeRateClientFactory exchangeFactory,
    IPublishEndpoint publishEndpoint, ICurrencyRepository currencyRepository, ICurrencyConverter currencyConverter,
    IUnitOfWork unitOfWork, IMediator mediator, ISettingsContainer settingsContainer) : ICommandHandler<UpdateCurrenciesRatesCommand>
{
    public async Task<Unit> Handle(UpdateCurrenciesRatesCommand request, CancellationToken cancellationToken)
    {
        CurrencySettings settings = settingsContainer.GetSetting(Abstractions.Constants.Settings.Currency);
        IExchangeRateClient provider = GetRateProvider(settings);
        Dictionary<string, Currency> currencies = await LoadCurrencies(cancellationToken);

        var convertedRates = await GetConvertedRates(provider, currencies, cancellationToken);

        var (history, changedRates, notFoundRates) 
            = ApplyRates(currencies, convertedRates);

        if (notFoundRates.Count > 0 && logger.IsEnabled(LogLevel.Warning))
            logger.LogWarning("Курсы валют для {@Currencies} не найдены у {Provider} на {Time}", notFoundRates, provider, DateTime.UtcNow);
        
        await PublishEvents(changedRates, cancellationToken);
        await SaveChanges(history, cancellationToken);

        return Unit.Value;
    }
    
    private IExchangeRateClient GetRateProvider(CurrencySettings settings)
    {
        return exchangeFactory.GetClient(settings.RateProvider);
    }
    
    private async Task<Dictionary<string, Currency>> LoadCurrencies(CancellationToken cancellationToken)
    {
        var list = await currencyRepository
            .GetCurrencies([], true, cancellationToken);

        return list.ToDictionary(x => x.Code, x => x);
    }
    
    private async Task<ExchangeRates> GetConvertedRates(IExchangeRateClient provider, Dictionary<string, Currency> currencies, 
        CancellationToken cancellationToken)
    {
        var usd = currencies.Values.FirstOrDefault(x => x.Id == Global.UsdId);
        if (usd == null)
            throw new CurrencyNotFoundException(Global.UsdId);

        var rates = await provider.GetRates(cancellationToken);

        return currencyConverter.ChangeBaseCurrency(rates, usd.Code);
    }
    
    private (List<CurrencyHistory> history, Dictionary<int, decimal> changedRates, List<string> notFoundRates) 
        ApplyRates(Dictionary<string, Currency> currencies, ExchangeRates convertedRates)
    {
        var history = new List<CurrencyHistory>();
        var changedRates = new Dictionary<int, decimal>();
        var notFoundRates = new List<string>();
        
        foreach (var currency in currencies.Values)
        {
            if (!convertedRates.Rates.TryGetValue(currency.Code, out var rate))
            {
                notFoundRates.Add(currency.Code);
                continue;
            }

            currency.CurrencyToUsd ??= new CurrencyToUsd
            {
                CurrencyId = currency.Id
            };

            var prevValue = currency.CurrencyToUsd.ToUsd;

            if (prevValue == rate)
                continue;

            currency.CurrencyToUsd.ToUsd = rate;

            history.Add(new CurrencyHistory
            {
                CurrencyId = currency.Id,
                PrevValue = prevValue,
                NewValue = rate
            });

            changedRates[currency.Id] = rate;
        }

        return (history, changedRates, notFoundRates);
    }
    
    private async Task SaveChanges(List<CurrencyHistory> history, CancellationToken cancellationToken)
    {
        if (history.Count > 0) await unitOfWork.AddRangeAsync(history, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
    
    private async Task PublishEvents(Dictionary<int, decimal> changedRates, CancellationToken cancellationToken)
    {
        if (changedRates.Count == 0) return;

        await publishEndpoint.Publish(new CurrencyRateChangedEvent { Rates = changedRates}, cancellationToken);
        await mediator.Publish(new CurrencyRatesUpdatedNotification(), cancellationToken);
    }

}