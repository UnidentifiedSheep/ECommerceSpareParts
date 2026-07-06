using Enums;
using Integrations.Common;
using Internal.Integration.Core.Models.Main.Product;

namespace Internal.Integration.Core.Interfaces.Main;

public interface IProductNode
{
    Task<Response<IReadOnlyList<InternalFullProduct>>> GetFullProduct(
        IEnumerable<int> productIds,
        CancellationToken cancellationToken = default);

    Task<Response<IReadOnlyList<InternalSupplierProductReference>>> GetSupplierProductReferences(
        IEnumerable<int> productIds,
        Supplier supplier,
        CancellationToken cancellationToken = default);

    Task<Response<IReadOnlyList<InternalSupplierProductReference>>> ResolveSupplierProductReferences(
        IEnumerable<InternalSupplierProductReferenceRequest> references,
        Supplier supplier,
        CancellationToken cancellationToken = default);
}
