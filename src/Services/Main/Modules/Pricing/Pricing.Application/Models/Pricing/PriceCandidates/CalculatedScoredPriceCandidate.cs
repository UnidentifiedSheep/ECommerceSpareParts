namespace Pricing.Application.Models.Pricing.PriceCandidates;

public record CalculatedScoredPriceCandidate : CalculatedPriceCandidate
{
	public required decimal Score { get; init; }

	public static CalculatedScoredPriceCandidate From(CalculatedPriceCandidate candidate, decimal score)
		=> new()
		{
			Score = score,
			AvailableQuantity = candidate.AvailableQuantity,
			DeliveryDays = candidate.DeliveryDays,
			GuaranteedDeliveryDays = candidate.GuaranteedDeliveryDays,
			DeliveryProbability = candidate.DeliveryProbability,
			Markup = candidate.Markup,
			Price = candidate.Price,
			Cost = candidate.Cost,
			CurrencyId = candidate.CurrencyId,
			ProductId = candidate.ProductId,
			PriceOfferId = candidate.PriceOfferId,
			SourceType = candidate.SourceType,
			StorageCode = candidate.StorageCode
		};
}
