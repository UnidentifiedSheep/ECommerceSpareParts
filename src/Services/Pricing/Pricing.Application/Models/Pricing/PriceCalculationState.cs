using Pricing.Application.Models.Pricing.PriceCandidates;

namespace Pricing.Application.Models.Pricing;

public sealed record PriceCalculationState
{
    public required PriceCandidate Candidate { get; init; }
    public required MarketInfo.MarketInfo Market { get; init; }
    
    public required int CurrencyId { get; init; }
    public required decimal Cost { get; init; }
    public required decimal SalePrice { get; init; }

    public MarkupResult? BaseMarkup { get; init; }

    public required IReadOnlyList<AppliedPriceRule> AppliedRules { get; init; }

    public static PriceCalculationState Initial(
        PriceCandidate candidate,
        MarketInfo.MarketInfo market)
    {
        return new PriceCalculationState
        {
            Candidate = candidate,
            Market = market,
            Cost = candidate.Cost,
            SalePrice = candidate.Cost,
            CurrencyId = candidate.CurrencyId,
            BaseMarkup = null,
            AppliedRules = []
        };
    }
}