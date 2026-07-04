using Integrations.Common;
using Integrations.Supplier.Connections;
using Integrations.Supplier.Models;
using Integrations.Supplier.Models.Requests;

namespace Integrations.Supplier.Interfaces;

public interface ISupplier
{
    global::Enums.Supplier Supplier { get; }

    Task<Response<IReadOnlyList<SupplierProduct>>> GetProductsAsync(
        GetProductsRequest request,
        CancellationToken cancellationToken = default);

    Task<ConnectionCheck> CheckConnectionAsync(CancellationToken cancellationToken = default);
}