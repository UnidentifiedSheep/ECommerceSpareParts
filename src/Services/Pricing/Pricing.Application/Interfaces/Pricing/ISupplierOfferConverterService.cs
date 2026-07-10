using Enums;
using Integrations.Supplier.Models;
using Pricing.Application.Models;

namespace Pricing.Application.Interfaces.Pricing;

public interface ISupplierOfferConverterService
{
    Task<IReadOnlyList<SupplierOfferConversionResult>> ConvertAsync(
        int productId,
        string storageName,
        IReadOnlyDictionary<Supplier, IReadOnlyList<SupplierPosition>> offers,
        CancellationToken token = default);

    Task<SupplierOfferConversionResult> ConvertAsync(
        int productId,
        string storageName,
        Supplier supplier,
        IReadOnlyList<SupplierPosition> positions,
        CancellationToken token = default);
}