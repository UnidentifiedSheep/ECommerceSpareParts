using Application.Common.Interfaces.Settings;
using Application.Common.Services.Currency;
using Main.Abstractions.Models.Settings;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Exceptions.Currencies;
using ZiggyCreatures.Caching.Fusion;

namespace Main.Application.Services.Currency;

public class CurrencyRatesProvider(
    IFusionCache cache,
    ICurrencyRateRepository rateRepository,
    ISettingsService settingsService) : CurrencyRatesProviderBase(cache)
{
    protected override async Task<decimal> GetExternalData(int currencyId, CancellationToken cancellationToken = default)
    {
        var baseCurrencyId = (await settingsService.GetOrDefault<CurrencySetting>(cancellationToken))
            .Data
            .BaseCurrencyId;

        if (currencyId == baseCurrencyId) return 1m;

        var rate = await rateRepository.GetById((currencyId, baseCurrencyId), cancellationToken)
                   ?? throw new CurrencyRateNotFoundException(currencyId);

        return rate.Rate;
    }
}