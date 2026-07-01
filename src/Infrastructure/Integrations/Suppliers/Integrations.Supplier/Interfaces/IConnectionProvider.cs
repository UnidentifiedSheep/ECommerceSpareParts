using Integrations.Supplier.Connections;

namespace Integrations.Supplier.Interfaces;

public interface IConnectionProvider<TModel>
{
    Task<TModel> GetConnectionAsync(CancellationToken cancellationToken = default);

    Task<ConnectionCheck<TModel>> CheckConnectionAsync(
        CancellationToken cancellationToken = default);
}