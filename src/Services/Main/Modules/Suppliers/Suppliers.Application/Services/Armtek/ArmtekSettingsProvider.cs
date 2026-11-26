using Application.Common.Interfaces.Settings;
using Integrations.Supplier.Interfaces;
using Integrations.Supplier.Settings;
using Main.Entities.Suppliers.Settings.Supplier;

namespace Main.Modules.Suppliers.Application.Services.Armtek;

public class ArmtekSettingsProvider(ISettingsService settingsService) : ISupplierSettingsProvider<ArmtekSettings>
{
	public async Task<ArmtekSettings> GetSettingsAsync(CancellationToken cancellationToken = default)
	{
		var settings = (await settingsService.GetOrDefault<ArmtekSupplierSetting>(cancellationToken)).Data;

		return new ArmtekSettings
		{
			Storages = settings
				.Storages
				.Select(x => new ArmtekStoragePoint
				{
					StorageCode = x.StorageCode,
					DeliveryMode = x.DeliveryMode,
					PointCode = x.PointCode
				})
				.ToList()
		};
	}
}
