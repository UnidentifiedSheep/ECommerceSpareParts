using Pricing.Application.Models.Pricing.PriceCandidates;

namespace Pricing.Application.Models.Pricing;

public sealed record PriceCalculationState
{
    public required PriceCandidate Candidate { get; init; }
    public required MarketInfo.MarketInfo Market { get; init; }

    public required decimal CostInBaseCurrency { get; init; }

    /// <summary>
    /// Текущая продажная цена после применённых правил.
    /// </summary>
    public required decimal SalePriceInBaseCurrency { get; init; }

    public required int BaseCurrencyId { get; init; }

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
            CostInBaseCurrency = candidate.CostInBaseCurrency,
            SalePriceInBaseCurrency = candidate.CostInBaseCurrency,
            BaseCurrencyId = candidate.BaseCurrencyId,
            BaseMarkup = null,
            AppliedRules = []
        };
    }
}