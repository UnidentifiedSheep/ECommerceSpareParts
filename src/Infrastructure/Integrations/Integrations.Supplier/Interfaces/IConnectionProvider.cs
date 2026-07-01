namespace Integrations.Supplier.Interfaces;

public interface IConnectionProvider
{
    Supplier Supplier { get; }
    Task<ConnectionModel> GetConnectionAsync(CancellationToken ct = default);
}