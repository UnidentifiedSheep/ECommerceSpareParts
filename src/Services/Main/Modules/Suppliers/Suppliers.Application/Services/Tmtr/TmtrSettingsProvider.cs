using Application.Common.Interfaces.Settings;
using Integrations.Supplier.Interfaces;
using Integrations.Supplier.Settings;
using Main.Entities.Suppliers.Settings.Supplier;

namespace Main.Modules.Suppliers.Application.Services.Tmtr;

public class TmtrSettingsProvider(ISettingsService settingsService) : ISupplierSettingsProvider<TmtrSettings>
{
	public async Task<TmtrSettings> GetSettingsAsync(CancellationToken cancellationToken = default)
	{
		var settings = (await settingsService.GetOrDefault<TmtrSupplierSetting>(cancellationToken)).Data;
		if (settings.GuaranteedDeliveryOffsetDays < 0)
			throw new InvalidOperationException("TMTR guaranteed delivery offset cannot be negative.");

		return new TmtrSettings
		{
			GuaranteedDeliveryOffsetDays = settings.GuaranteedDeliveryOffsetDays
		};
	}
}
