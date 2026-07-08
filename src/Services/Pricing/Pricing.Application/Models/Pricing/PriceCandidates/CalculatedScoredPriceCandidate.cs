namespace Pricing.Application.Models.Pricing.PriceCandidates;

public record CalculatedScoredPriceCandidate : CalculatedPriceCandidate
{
    public required decimal Score { get; init; }

    public static CalculatedScoredPriceCandidate From(CalculatedPriceCandidate candidate, decimal score)
    {
        return new CalculatedScoredPriceCandidate
        {
            Score = score,
            AvailableQuantity = candidate.AvailableQuantity,
            DeliveryTime = candidate.DeliveryTime,
            GuaranteedDeliveryTime = candidate.GuaranteedDeliveryTime,
            DeliveryProbability = candidate.DeliveryProbability,
            Markup = candidate.Markup,
            PriceInBaseCurrency = candidate.PriceInBaseCurrency,
            ProductId = candidate.ProductId,
            PriceOfferId = candidate.PriceOfferId,
            SourceType = candidate.SourceType,
            StorageName = candidate.StorageName,
            CostInBaseCurrency = candidate.CostInBaseCurrency
        };
    }
}