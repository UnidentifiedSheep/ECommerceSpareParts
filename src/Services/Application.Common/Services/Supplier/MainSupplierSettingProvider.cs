using Abstractions;
using Application.Common.Static;
using Integrations.Supplier.Enums;
using Internal.Integration.Core.Interfaces.Common;
using ZiggyCreatures.Caching.Fusion;

namespace Application.Common.Services.Supplier;

public abstract class MainSupplierSettingProvider<TSetting>(
    IFusionCache cache,
    ICommonClient commonClient,
    string settingSystemName,
    string cacheKey)
    where TSetting : class
{
    public async Task<MainSupplierSettingResult<TSetting>> GetAsync(
        CancellationToken cancellationToken = default)
    {
        return await cache.GetOrSetAsync(
            cacheKey,
            LoadAsync,
            new FusionCacheEntryOptions(CacheKeys.SettingsCache.Ttl),
            cancellationToken);
    }

    private async Task<MainSupplierSettingResult<TSetting>> LoadAsync(
        CancellationToken cancellationToken)
    {
        var response = await commonClient.SettingNode.GetSetting(
            ServicesDefinitions.Main,
            settingSystemName,
            cancellationToken);

        if (!response.Success)
            return MainSupplierSettingResult<TSetting>.Failure(
                SupplierUnavailableReason.SettingsUnavailable,
                $"Unable to get {settingSystemName}");

        try
        {
            var setting = System.Text.Json.JsonSerializer.Deserialize<TSetting>(
                response.ValueOrThrow);

            return setting is null
                ? MainSupplierSettingResult<TSetting>.Failure(
                    SupplierUnavailableReason.InvalidConfiguration,
                    $"Invalid {settingSystemName} JSON")
                : MainSupplierSettingResult<TSetting>.Success(setting);
        }
        catch (System.Text.Json.JsonException)
        {
            return MainSupplierSettingResult<TSetting>.Failure(
                SupplierUnavailableReason.InvalidConfiguration,
                $"Invalid {settingSystemName} JSON");
        }
    }
}

public sealed record MainSupplierSettingResult<TSetting>(
    TSetting? Setting,
    SupplierUnavailableReason? Reason = null,
    string? Message = null)
    where TSetting : class
{
    public bool IsSuccess => Setting is not null;

    public static MainSupplierSettingResult<TSetting> Success(TSetting setting)
    {
        return new MainSupplierSettingResult<TSetting>(setting);
    }

    public static MainSupplierSettingResult<TSetting> Failure(
        SupplierUnavailableReason reason,
        string message)
    {
        return new MainSupplierSettingResult<TSetting>(
            default,
            reason,
            message);
    }
}
