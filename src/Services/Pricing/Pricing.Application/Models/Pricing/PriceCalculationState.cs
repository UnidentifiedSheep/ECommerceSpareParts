using System.Text.Json.Serialization;
using Pricing.Application.Models.Pricing.PriceCandidates;

namespace Pricing.Application.Models.Pricing;

public sealed record PriceCalculationState
{
    [JsonPropertyName("candidate")]
    public required PriceCandidate Candidate { get; init; }

    [JsonPropertyName("market")]
    public required MarketInfo.MarketInfo Market { get; init; }

    [JsonPropertyName("currencyId")]
    public required int CurrencyId { get; init; }

    [JsonPropertyName("cost")]
    public required decimal Cost { get; init; }

    [JsonPropertyName("salePrice")]
    public required decimal SalePrice { get; init; }

    [JsonPropertyName("baseMarkup")]
    public MarkupResult? BaseMarkup { get; init; }

    [JsonPropertyName("appliedRules")]
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
