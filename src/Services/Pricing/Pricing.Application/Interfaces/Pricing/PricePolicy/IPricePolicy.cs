using Pricing.Application.Models.Pricing;
using Pricing.Application.Models.Pricing.MarketInfo;
using Pricing.Application.Models.Pricing.PriceCandidates;
using Pricing.Enums;

namespace Pricing.Application.Interfaces.Pricing.PricePolicy;

public interface IPricePolicy
{
    PriceOfferSourceType SourceType { get; }

    Task<IReadOnlyCollection<CalculatedPriceCandidate>> CalculateAsync(
        IReadOnlyCollection<PriceCandidate> candidates,
        MarketInfo market,
        CancellationToken ct);
}