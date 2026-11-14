using System.Text.Json;
using Application.Common.Static;
using Integrations.Supplier.Enums;
using ZiggyCreatures.Caching.Fusion;

namespace Application.Common.Services.Supplier;

public abstract class MainSupplierSettingProvider<TSetting>(
	IFusionCache cache,
	ISupplierSettingSource settingSource,
	string settingSystemName,
	string cacheKey) where TSetting : class
{
	public async Task<MainSupplierSettingResult<TSetting>> GetAsync(
		CancellationToken cancellationToken = default)
		=> await cache.GetOrSetAsync(
			cacheKey,
			LoadAsync,
			new FusionCacheEntryOptions(CacheKeys.SettingsCache.Ttl),
			cancellationToken);

	private async Task<MainSupplierSettingResult<TSetting>> LoadAsync(CancellationToken cancellationToken)
	{
		var json = await settingSource.GetSettingJsonAsync(settingSystemName, cancellationToken);

		if (json is null)
			return MainSupplierSettingResult<TSetting>.Failure(
				SupplierUnavailableReason.SettingsUnavailable,
				$"Unable to get {settingSystemName}");

		try
		{
			var setting = JsonSerializer.Deserialize<TSetting>(json);

			return setting is null
				? MainSupplierSettingResult<TSetting>.Failure(
					SupplierUnavailableReason.InvalidConfiguration,
					$"Invalid {settingSystemName} JSON")
				: MainSupplierSettingResult<TSetting>.Success(setting);
		}
		catch (JsonException)
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
	string? Message = null) where TSetting : class
{
	public bool IsSuccess => Setting is not null;

	public static MainSupplierSettingResult<TSetting> Success(TSetting setting) => new(setting);

	public static MainSupplierSettingResult<TSetting> Failure(
		SupplierUnavailableReason reason,
		string message)
		=> new(
			default,
			reason,
			message);
}
