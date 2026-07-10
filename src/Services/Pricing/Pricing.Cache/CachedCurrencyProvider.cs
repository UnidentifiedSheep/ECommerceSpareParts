using System.Text.Json;
using System.Text.Json.Serialization;
using Abstractions;
using Internal.Integration.Core.Interfaces.Common;
using Internal.Integration.Core.Interfaces.Main;
using Pricing.Application.Interfaces.Cache;
using Pricing.Application.Static;
using ZiggyCreatures.Caching.Fusion;

namespace Pricing.Cache;

public class CachedCurrencyProvider(
    IFusionCache fusionCache,
    IMainClient mainClient,
    ICommonClient commonClient
) : ICachedCurrencyProvider
{
    public async Task<decimal?> GetCurrencyRate(int currencyId, CancellationToken cancellationToken = default)
    {
        var key = CacheKeys.Currency.CurrencyRate(currencyId);
        var cached = await fusionCache.TryGetAsync<decimal>(key, token: cancellationToken);
        if (cached.HasValue) return cached.Value;

        var response = await mainClient.CurrencyNode.GetCurrencyRate(currencyId, cancellationToken);
        decimal? value = response.Success ? response.ValueOrThrow : null;

        if (value == null) return null;
        await fusionCache.SetAsync(
            key,
            value,
            CacheKeys.Currency.Ttl,
            cancellationToken);
        return value;
    }

    public async Task InvalidateCurrencyRate(int currencyId, CancellationToken cancellationToken = default)
    {
        await fusionCache.RemoveAsync(
            CacheKeys.Currency.CurrencyRate(currencyId),
            token: cancellationToken);
    }

    public async Task<int?> GetCurrencyIdAsync(string code, CancellationToken token = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();

        var fromCache = await fusionCache.TryGetAsync<int>(
            CacheKeys.Currency.CurrencyIdByCode(normalizedCode),
            token: token);

        if (fromCache.HasValue)
            return fromCache.Value;

        var response = await mainClient.CurrencyNode.GetCurrencies(token);
        if (!response.Success)
            throw new InvalidOperationException("Unable to get currencies from main service");

        int? result = null;

        foreach (var item in response.ValueOrThrow)
        {
            var itemCode = item.Code.Trim().ToUpperInvariant();

            if (itemCode == normalizedCode)
                result = item.Id;

            await fusionCache.SetAsync(
                key: CacheKeys.Currency.CurrencyIdByCode(itemCode),
                value: item.Id,
                options: new FusionCacheEntryOptions(CacheKeys.Currency.Ttl),
                token: token);
        }

        return result;
    }
    public ValueTask<int> GetBaseCurrencyIdAsync(CancellationToken token = default)
    {
        return fusionCache.GetOrSetAsync(
            key: CacheKeys.Currency.BaseCurrencyId,
            async ct =>
            {
                var setting = await commonClient.SettingNode.GetSetting(
                    ServicesDefinitions.Main,
                    "CurrencySetting",
                    ct);
                
                if (!setting.Success)
                    throw new InvalidOperationException("Unable to get base currency id");
                
                var data = JsonSerializer.Deserialize<CurrencySettingData>(setting.ValueOrThrow)
                    ?? throw new InvalidOperationException("Unable to deserialize currency setting");

                return data.BaseCurrencyId;
            },
            options: new FusionCacheEntryOptions(CacheKeys.Currency.Ttl),
            token: token);
        
    }

    private record CurrencySettingData
    {
        [JsonPropertyName("baseCurrencyId")]
        public int BaseCurrencyId { get; init; }
    }
}