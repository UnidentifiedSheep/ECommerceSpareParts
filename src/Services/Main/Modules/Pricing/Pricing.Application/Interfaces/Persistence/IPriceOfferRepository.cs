using Application.Common.Interfaces.Repositories;
using Pricing.Entities.Enums;
using Pricing.Entities.Offers;

namespace Pricing.Application.Interfaces.Persistence;

public interface IPriceOfferRepository : IRepository<PriceOffer, Guid>
{
	Task UpsertOffersAsync(IEnumerable<PriceOffer> offers, CancellationToken cancellationToken = default);

	Task DeleteOffersAsync(
		int productId,
		string storageCode,
		IEnumerable<PriceOfferSource> sources,
		CancellationToken cancellationToken = default);
}
