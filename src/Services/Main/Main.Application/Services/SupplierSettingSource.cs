using System.Text.Json;
using Application.Common.Interfaces.Settings;
using Application.Common.Services.Supplier;
using Main.Entities.Settings.Supplier;

namespace Main.Application.Services;

public sealed class SupplierSettingSource(ISettingsService settingsService) : ISupplierSettingSource
{
	public async Task<string?> GetSettingJsonAsync(
		string settingSystemName,
		CancellationToken cancellationToken = default)
	{
		if (settingSystemName == FavoritSupplierSetting.SettingName)
			return JsonSerializer.Serialize(
				(await settingsService.GetOrDefault<FavoritSupplierSetting>(cancellationToken)).Data);

		if (settingSystemName == TmtrSupplierSetting.SettingName)
			return JsonSerializer.Serialize(
				(await settingsService.GetOrDefault<TmtrSupplierSetting>(cancellationToken)).Data);

		return null;
	}
}
