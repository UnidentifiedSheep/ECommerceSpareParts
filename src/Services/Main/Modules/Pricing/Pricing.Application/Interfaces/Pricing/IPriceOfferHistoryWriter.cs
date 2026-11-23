using Pricing.Entities.Enums;
using Pricing.Entities.Offers;

namespace Pricing.Application.Interfaces.Pricing;

public interface IPriceOfferHistoryWriter
{
	Task TrackAsync(
		string storageCode,
		PriceOfferSource source,
		IReadOnlyCollection<int> productIds,
		IReadOnlyCollection<PriceOffer> offers,
		DateTime seenAt,
		CancellationToken cancellationToken = default);
}
