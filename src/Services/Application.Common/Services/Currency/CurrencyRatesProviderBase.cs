using Application.Common.Interfaces.Currency;
using ZiggyCreatures.Caching.Fusion;

namespace Application.Common.Services.Currency;

public abstract class CurrencyRatesProviderBase(
    IFusionCache cache) : ICurrencyRatesProvider
{
    public virtual async Task<decimal> GetRate(int currencyId, CancellationToken cancellationToken = default)
    {
        return await cache.GetOrSetAsync(
            key: $"currency:{currencyId}:rate",
            factory: ct => GetFromDb(currencyId, ct),
            duration: TimeSpan.FromDays(1),
            tags: [$"currency:{currencyId}"],
            token: cancellationToken);
    }
    
    protected abstract Task<decimal> GetFromDb(int currencyId, CancellationToken cancellationToken = default);
}