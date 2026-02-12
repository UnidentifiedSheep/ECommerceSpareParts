using Contracts.Currency.GetCurrencies;
using Mapster;
using MassTransit;
using Pricing.Abstractions.Interfaces.CacheRepositories;
using Pricing.Abstractions.Interfaces.Services;
using Pricing.Abstractions.Models;

namespace Pricing.Application.Services;

public class CurrencyService(ICurrencyCacheRepository currencyCache, IRequestClient<GetCurrenciesRequest> requestClient)
    : ICurrencyService
{
    private Task<List<Currency>>? _loadingTask;
    private readonly Lock _lock = new();

    public async Task<List<Currency>> GetCurrencies(CancellationToken cancellationToken = default)
    {
        var cached = await currencyCache.GetCurrencies() ?? await EnsureLoaded(cancellationToken);
        return cached;
    }

    public Task<List<Currency>> ReloadCurrencies(CancellationToken cancellationToken = default)
    {
        return EnsureLoaded(cancellationToken, forceReload: true);
    }

    private Task<List<Currency>> EnsureLoaded(CancellationToken cancellationToken, bool forceReload = false)
    {
        lock (_lock)
        {
            if (forceReload) _loadingTask = null;

            _loadingTask ??= ReloadInternal(cancellationToken);

            return _loadingTask;
        }
    }

    private async Task<List<Currency>> ReloadInternal(CancellationToken cancellationToken)
    {
        try
        {
            var response = await requestClient.GetResponse<GetCurrenciesResponse>(new GetCurrenciesRequest(), cancellationToken);

            var currencies = response.Message.Currencies.Adapt<List<Currency>>();
            await currencyCache.SetCurrencies(currencies);

            return currencies;
        }
        catch
        {
            lock (_lock)
                _loadingTask = null;
            
            throw;
        }
    }
}

