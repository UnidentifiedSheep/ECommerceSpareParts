using Integrations.Supplier.Connections;

namespace Integrations.Supplier.Interfaces;

public interface IConnectionProvider<TModel> : IConnectionProvider
{
    Task<TModel> GetConnectionAsync(CancellationToken cancellationToken = default);

    new Task<ConnectionCheck<TModel>> CheckConnectionAsync(
        CancellationToken cancellationToken = default);
}

public interface IConnectionProvider
{
    Task<ConnectionCheck> CheckConnectionAsync(
        CancellationToken cancellationToken = default);
}