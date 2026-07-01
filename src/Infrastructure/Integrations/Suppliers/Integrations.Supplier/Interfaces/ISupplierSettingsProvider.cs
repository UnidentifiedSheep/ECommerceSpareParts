namespace Integrations.Supplier.Interfaces;

public interface ISupplierSettingsProvider<TModel>
{
    Task<TModel> GetSettingsAsync(CancellationToken cancellationToken = default);
}