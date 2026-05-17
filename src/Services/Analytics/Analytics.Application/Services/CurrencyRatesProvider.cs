using Application.Common.Services.Currency;
using Internal.Integration.Core.Interfaces;
using ZiggyCreatures.Caching.Fusion;

namespace Analytics.Application.Services;

public class CurrencyRatesProvider(
    IFusionCache cache,
    IMainClient mainClient) : CurrencyRatesProviderBase(cache)
{
    protected override Task<decimal> GetExternalData(
        int currencyId,
        CancellationToken cancellationToken = default)
        => mainClient.GetCurrencyRate(currencyId, cancellationToken);
}