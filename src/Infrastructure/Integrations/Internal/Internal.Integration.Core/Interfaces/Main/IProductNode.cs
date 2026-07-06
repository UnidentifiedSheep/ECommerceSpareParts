using Enums;
using Integrations.Common;
using Internal.Integration.Core.Models.Main.Product;

namespace Internal.Integration.Core.Interfaces.Main;

public interface IProductNode
{
    Task<Response<IReadOnlyList<InternalFullProduct>>> GetFullProduct(
        IEnumerable<int> productIds,
        CancellationToken cancellationToken = default);

    Task<Response<IReadOnlyList<InternalSupplierProductResolvedReference>>> GetSupplierProductReferences(
        IEnumerable<int> productIds,
        Supplier supplier,
        CancellationToken cancellationToken = default);

    Task<Response<Dictionary<Supplier, IReadOnlyList<InternalSupplierProductResolvedReference>>>> ResolveSupplierProductReferences(
        Dictionary<Supplier, IEnumerable<InternalSupplierProductReferenceLookup>> references,
        CancellationToken cancellationToken = default);
}
