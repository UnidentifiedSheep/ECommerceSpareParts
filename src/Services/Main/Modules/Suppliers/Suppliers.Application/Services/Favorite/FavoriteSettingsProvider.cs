using Integrations.Supplier.Interfaces;
using Integrations.Supplier.Settings;

namespace Main.Modules.Suppliers.Application.Services.Favorite;

public class FavoriteSettingsProvider : ISupplierSettingsProvider<FavoriteSettings>
{
	public Task<FavoriteSettings> GetSettingsAsync(CancellationToken cancellationToken = default)
		=> Task.FromResult(
			new FavoriteSettings
			{
				MaxDaysToDelivery = 2, MinDaysToDelivery = 1
			}); //TODO: settings should be taken from main.
}
