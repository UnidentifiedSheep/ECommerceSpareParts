namespace Integrations.Supplier.Interfaces;

public interface IConnectionProvider<TModel>
{
    Task<TModel> GetConnectionAsync(CancellationToken cancellationToken = default);
}