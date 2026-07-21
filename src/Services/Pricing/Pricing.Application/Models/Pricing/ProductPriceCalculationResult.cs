using Pricing.Application.Models.Pricing.PriceCandidates;

namespace Pricing.Application.Models.Pricing;

public record ProductPriceCalculationResult
{
    public required string MarkupVersion { get; init; }
    public required string AppliersVersion { get; init; }
    public required Guid PricingSettingsVersion { get; init; }
    public required IReadOnlyCollection<CalculatedScoredPriceCandidate> Candidates { get; init; }
}
