using Enums;
using Integrations.Supplier.Models;
using Pricing.Entities;

namespace Pricing.Application.Interfaces.Pricing;

public interface IOfferRefreshService
{
    Task<IReadOnlyList<PriceOffer>> RefreshOffersAsync(
        int productId,
        string storageName,
        CancellationToken token = default);

    Task<IReadOnlyList<PriceOffer>> RefreshOffersAsync(
        int productId,
        string storageName,
        IReadOnlyDictionary<Supplier, IReadOnlyList<SupplierPosition>> supplierPositions,
        CancellationToken token = default);

    Task<IReadOnlyList<PriceOffer>> RefreshOffersAsync(
        DateTime dataExtractionTime,
        string storageName,
        Supplier supplier,
        IReadOnlyDictionary<int, IReadOnlyList<SupplierPosition>> supplierPositions,
        CancellationToken token = default);
}