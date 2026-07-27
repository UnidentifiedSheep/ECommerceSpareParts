using Abstractions;
using Application.Common.Static;
using Integrations.Supplier.Interfaces;
using Integrations.Supplier.Settings;
using Internal.Integration.Core.Interfaces.Common;
using ZiggyCreatures.Caching.Fusion;

namespace Application.Common.Services.Supplier.Tmtr;

public class TmtrSettingsProvider(
    IFusionCache cache,
    ICommonClient commonClient
) : ISupplierSettingsProvider<TmtrSettings>
{
    private const string SettingSystemName = "TmtrSupplierSetting";

    public async Task<TmtrSettings> GetSettingsAsync(
        CancellationToken cancellationToken = default)
    {
        return await cache.GetOrSetAsync(
            CacheKeys.SettingsCache.TmtrSettings,
            LoadSettingsAsync,
            new FusionCacheEntryOptions(CacheKeys.SettingsCache.Ttl),
            cancellationToken);
    }

    private async Task<TmtrSettings> LoadSettingsAsync(
        CancellationToken cancellationToken)
    {
        var response = await commonClient.SettingNode.GetSetting(
            ServicesDefinitions.Main,
            SettingSystemName,
            cancellationToken);

        if (!response.Success)
            throw new InvalidOperationException("Unable to get TMTR settings.");

        var settings = System.Text.Json.JsonSerializer.Deserialize<TmtrMainSettings>(
            response.ValueOrThrow)
            ?? throw new InvalidOperationException("Invalid TMTR settings JSON.");

        if (settings.GuaranteedDeliveryOffsetDays < 0)
            throw new InvalidOperationException(
                "TMTR guaranteed delivery offset cannot be negative.");

        return new TmtrSettings
        {
            GuaranteedDeliveryOffsetDays = settings.GuaranteedDeliveryOffsetDays
        };
    }
}
