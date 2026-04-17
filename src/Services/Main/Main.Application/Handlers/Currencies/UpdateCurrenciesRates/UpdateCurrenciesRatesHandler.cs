using Abstractions.Interfaces.Currency;
using Abstractions.Interfaces.Integrations.ExchangeRate;
using Abstractions.Interfaces.Services;
using Abstractions.Models;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Settings;
using Attributes;
using Contracts.Currency;
using Main.Abstractions.Exceptions.Currencies;
using Main.Abstractions.Models.Settings;
using Main.Application.Notifications;
using Main.Entities;
using Main.Entities.Currency;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Main.Application.Handlers.Currencies.UpdateCurrenciesRates;

[AutoSave]
[Transactional]
public record UpdateCurrenciesRatesCommand : ICommand;

public class UpdateCurrenciesRatesHandler(
    ILogger<UpdateCurrenciesRatesCommand> logger,
    IExchangeRateClientFactory exchangeFactory,
    IPublishEndpoint publishEndpoint,
    IRepository<Currency, int> currencyRepository,
    ICurrencyConverter currencyConverter,
    IPublisher publisher,
    ISettingsContainer settingsContainer) : ICommandHandler<UpdateCurrenciesRatesCommand>
{
    public async Task<Unit> Handle(UpdateCurrenciesRatesCommand request, CancellationToken cancellationToken)
    {
        var settings = settingsContainer.GetSetting(Abstractions.Constants.Settings.Currency);
        var provider = GetRateProvider(settings);
        var currencies = await LoadCurrencies(cancellationToken);

        var convertedRates = await GetConvertedRates(provider, currencies, cancellationToken);

        var (changedRates, notFoundRates)
            = ApplyRates(currencies, convertedRates);

        if (notFoundRates.Count > 0)
            logger.LogWarning("Курсы валют для {@Currencies} не найдены у {Provider} на {Time}", notFoundRates,
                provider, DateTime.UtcNow);

        await PublishEvents(changedRates, cancellationToken);

        return Unit.Value;
    }

    private IExchangeRateClient GetRateProvider(CurrencySettings settings)
    {
        return exchangeFactory.GetClient(settings.RateProvider);
    }

    private async Task<Dictionary<string, Currency>> LoadCurrencies(CancellationToken cancellationToken)
    {
        var list = await currencyRepository.ListAsync(ct: cancellationToken);

        return list.ToDictionary(x => x.Code, x => x);
    }

    private async Task<ExchangeRates> GetConvertedRates(
        IExchangeRateClient provider,
        Dictionary<string, Currency> currencies,
        CancellationToken cancellationToken)
    {
        var usd = currencies.Values.FirstOrDefault(x => x.Id == Global.UsdId);
        if (usd == null)
            throw new CurrencyNotFoundException(Global.UsdId);

        var rates = await provider.GetRates(cancellationToken);

        return currencyConverter.ChangeBaseCurrency(rates, usd.Code);
    }

    private (Dictionary<int, decimal> changedRates, List<string> notFoundRates)
        ApplyRates(Dictionary<string, Currency> currencies, ExchangeRates convertedRates)
    {
        var changedRates = new Dictionary<int, decimal>();
        var notFoundRates = new List<string>();

        foreach (var currency in currencies.Values)
        {
            if (!convertedRates.Rates.TryGetValue(currency.Code, out var rate))
            {
                notFoundRates.Add(currency.Code);
                continue;
            }

            var prevValue = currency.CurrencyToUsd?.ToUsd ?? 0;

            if (prevValue == rate)
                continue;

            currency.SetCurrencyToUsd(rate);

            changedRates[currency.Id] = rate;
        }

        return (changedRates, notFoundRates);
    }

    private async Task PublishEvents(Dictionary<int, decimal> changedRates, CancellationToken cancellationToken)
    {
        if (changedRates.Count == 0) return;

        await publishEndpoint.Publish(new CurrencyRateChangedEvent { Rates = changedRates }, cancellationToken);
        await publisher.Publish(new CurrencyRatesUpdatedNotification(), cancellationToken);
    }
}