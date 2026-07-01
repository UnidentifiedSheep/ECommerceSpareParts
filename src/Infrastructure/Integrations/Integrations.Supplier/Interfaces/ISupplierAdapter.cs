using Integrations.Common;
using Integrations.Supplier.Models;
using Integrations.Supplier.Models.Requests;

namespace Integrations.Supplier.Interfaces;

public interface ISupplierAdapter
{
    Supplier Supplier { get; }
    
    Task<Response<IReadOnlyList<SupplierProduct>>> GetProductsAsync(
        GetProductsRequest request,
        CancellationToken cancellationToken = default);
}