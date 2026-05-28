using Abstractions.Interfaces.Integrations.ExchangeRate;
using Abstractions.Interfaces.Persistence;
using Abstractions.Interfaces.Services;
using Abstractions.Models;
using Application.Common.Interfaces.Currency;
using Application.Common.Interfaces.Repositories;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Services.Currency;
using Main.Application.Models.Currency;
using Main.Entities.Currency;
using Main.Entities.Exceptions;
using Main.Entities.Setting;

namespace Main.Application.Services.Currency;

public class CurrencyRateUpdater(
    IExchangeRateClientFactory clientFactory,
    IRepository<Entities.Currency.Currency, int> currencyRepository,
    ICurrencyRateRepository rateRepository,
    ICurrencyConverter converter,
    IUnitOfWork unitOfWork) : ICurrencyRateUpdater
{
    public async Task<UpdateRatesResult> UpdateAsync(CurrencySetting setting, CancellationToken ct)
    {
        var baseCurrency = await currencyRepository.GetById(setting.Data.BaseCurrencyId, ct)
                           ?? throw new CurrencyNotFoundException(setting.Data.BaseCurrencyId);

        var client = clientFactory.GetClient(setting.Data.RateProvider);

        var externalRates = await client.GetRates(ct);

        var normalized = converter.ChangeBaseCurrency(
            externalRates,
            baseCurrency.Code);

        var currencies = await currencyRepository.ListAsync(ct: ct);

        var dbRates = (await rateRepository
                .GetByBaseCurrency(baseCurrency.Id, null, ct))
            .ToDictionary(x => x.GetId());

        return ApplyDiff(currencies, baseCurrency, normalized, dbRates, ct);
    }

    private UpdateRatesResult ApplyDiff(
        List<Entities.Currency.Currency> currencies,
        Entities.Currency.Currency baseCurrency,
        ExchangeRates external,
        Dictionary<(int, int), CurrencyRate> dbRates,
        CancellationToken ct)
    {
        var changed = new Dictionary<int, decimal>();
        var notFound = new List<string>();
        var toAdd = new List<CurrencyRate>();

        foreach (var currency in currencies)
        {
            if (currency.Id == baseCurrency.Id)
                continue;

            if (!external.Rates.TryGetValue(currency.Code, out var rate))
            {
                notFound.Add(currency.Code);
                continue;
            }

            if (dbRates.TryGetValue((currency.Id, baseCurrency.Id), out var existing))
                existing.SetRate(rate);
            else
                toAdd.Add(CurrencyRate.Create(currency.Id, baseCurrency.Id, rate));

            changed[currency.Id] = rate;
        }

        if (toAdd.Count > 0)
            unitOfWork.AddRangeAsync(toAdd, ct);

        return new UpdateRatesResult(changed, notFound);
    }
}