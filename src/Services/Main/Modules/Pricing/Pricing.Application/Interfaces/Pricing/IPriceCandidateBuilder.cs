using Pricing.Application.Models.Pricing.PriceCandidates;
using Pricing.Entities.Offers;

namespace Pricing.Application.Interfaces.Pricing;

public interface IPriceCandidateBuilder
{
	Task<IReadOnlyCollection<PriceCandidate>> Build(
		IReadOnlyCollection<PriceOffer> offers,
		string targetStorageCode,
		CancellationToken cancellationToken = default);
}
