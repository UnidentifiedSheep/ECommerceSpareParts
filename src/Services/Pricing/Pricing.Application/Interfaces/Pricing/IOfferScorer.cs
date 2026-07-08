using Pricing.Application.Models.Pricing;
using Pricing.Application.Models.Pricing.PriceCandidates;

namespace Pricing.Application.Interfaces.Pricing;

public interface IOfferScorer
{
    ValueTask<decimal> GetCostScoreAsync(
        OfferCostScoreContext context,
        CancellationToken cancellationToken = default);
    
    ValueTask<IReadOnlyList<CalculatedScoredPriceCandidate>> GetResultingScoreAsync(
        IReadOnlyList<CalculatedPriceCandidate> candidates,
        CancellationToken cancellationToken = default);
}