using Integrations.Supplier.Interfaces;
using Integrations.Supplier.Settings;

namespace Application.Common.Services.Supplier.Tmtr;

public class TmtrSettingsProvider(TmtrMainSettingProvider settingsProvider)
	: ISupplierSettingsProvider<TmtrSettings>
{
	public async Task<TmtrSettings> GetSettingsAsync(CancellationToken cancellationToken = default)
	{
		var result = await settingsProvider.GetAsync(cancellationToken);
		if (!result.IsSuccess)
			throw new InvalidOperationException(result.Message ?? "Unable to get TMTR settings.");

		var settings = result.Setting!;
		if (settings.GuaranteedDeliveryOffsetDays < 0)
			throw new InvalidOperationException("TMTR guaranteed delivery offset cannot be negative.");

		return new TmtrSettings
		{
			GuaranteedDeliveryOffsetDays = settings.GuaranteedDeliveryOffsetDays
		};
	}
}
