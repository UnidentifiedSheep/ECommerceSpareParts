using Enums;
using Integrations.Supplier.Models;
using Pricing.Entities.Offers;

namespace Pricing.Application.Interfaces.Pricing;

public interface IOfferRefreshService
{
	Task<IReadOnlyList<PriceOffer>> RefreshOffersAsync(
		int productId,
		string storageCode,
		CancellationToken token = default);

	Task<IReadOnlyList<PriceOffer>> RefreshOffersAsync(
		int productId,
		string storageCode,
		IReadOnlyDictionary<Supplier, IReadOnlyList<SupplierPosition>> supplierPositions,
		CancellationToken token = default);

	Task<IReadOnlyList<PriceOffer>> RefreshOffersAsync(
		DateTime dataExtractionTime,
		string storageCode,
		Supplier supplier,
		IReadOnlyDictionary<int, IReadOnlyList<SupplierPosition>> supplierPositions,
		CancellationToken token = default);
}
