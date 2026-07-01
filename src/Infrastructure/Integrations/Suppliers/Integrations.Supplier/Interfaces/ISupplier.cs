using Integrations.Common;
using Integrations.Supplier.Models;
using Integrations.Supplier.Models.Requests;

namespace Integrations.Supplier.Interfaces;

public interface ISupplier
{
    Supplier Supplier { get; }
    
    Task<Response<IReadOnlyList<SupplierProduct>>> GetProductsAsync(
        GetProductsRequest request,
        CancellationToken cancellationToken = default);
}