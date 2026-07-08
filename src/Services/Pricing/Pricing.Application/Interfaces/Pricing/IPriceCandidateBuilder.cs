using Pricing.Application.Models.Pricing;
using Pricing.Application.Models.Pricing.PriceCandidates;
using Pricing.Entities;

namespace Pricing.Application.Interfaces.Pricing;

public interface IPriceCandidateBuilder
{
    Task<IReadOnlyCollection<PriceCandidate>> Build(
        IReadOnlyCollection<PriceOffer> offers,
        string targetStorageName,
        CancellationToken cancellationToken = default);
}