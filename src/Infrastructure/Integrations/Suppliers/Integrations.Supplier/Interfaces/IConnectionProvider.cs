namespace Integrations.Supplier.Interfaces;

public interface IConnectionProvider<TModel> : IConnectionProvider
{
    new Task<TModel> GetConnectionAsync(CancellationToken cancellationToken = default);
}

public interface IConnectionProvider
{
    Supplier Supplier { get; }
    Task<object> GetConnectionAsync(CancellationToken cancellationToken = default);
}