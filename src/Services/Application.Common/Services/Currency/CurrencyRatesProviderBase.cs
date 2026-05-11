using Application.Common.Interfaces.Currency;
using ZiggyCreatures.Caching.Fusion;

namespace Application.Common.Services.Currency;

public abstract class CurrencyRatesProviderBase(
    IFusionCache cache) : ICurrencyRatesProvider
{
    public virtual async Task<decimal> GetRate(int currencyId, CancellationToken cancellationToken = default)
    {
        return await cache.GetOrSetAsync(
            $"currency:{currencyId}:rate",
            ct => GetExternalData(currencyId, ct),
            TimeSpan.FromDays(1),
            [$"currency:{currencyId}"],
            cancellationToken);
    }

    protected abstract Task<decimal> GetExternalData(int currencyId, CancellationToken cancellationToken = default);
}