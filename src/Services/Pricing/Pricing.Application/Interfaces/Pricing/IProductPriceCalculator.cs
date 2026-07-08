using Pricing.Application.Models.Pricing;
using Pricing.Application.Models.Pricing.PriceCandidates;

namespace Pricing.Application.Interfaces.Pricing;

public interface IProductPriceCalculator
{
    Task<IReadOnlyCollection<CalculatedScoredPriceCandidate>> CalculateAsync(
        IReadOnlyCollection<PriceCandidate> candidates,
        CancellationToken ct);
}