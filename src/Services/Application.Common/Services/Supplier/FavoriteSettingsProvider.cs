using Integrations.Supplier.Interfaces;
using Integrations.Supplier.Settings;

namespace Application.Common.Services.Supplier;

public class FavoriteSettingsProvider : ISupplierSettingsProvider<FavoriteSettings>
{
    public Task<FavoriteSettings> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            new FavoriteSettings
            {
                MaxDaysToDelivery = 7,
                MinDaysToDelivery = 1
            }); //TODO: settings should be taken from main.
    }
}