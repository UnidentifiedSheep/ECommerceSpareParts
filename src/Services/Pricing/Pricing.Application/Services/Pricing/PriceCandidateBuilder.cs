using Pricing.Application.Extensions;
using Pricing.Application.Interfaces.Pricing;
using Pricing.Application.Models.Pricing;
using Pricing.Application.Models.Pricing.PriceCandidates;
using Pricing.Entities.Offers;
using Pricing.Enums;

namespace Pricing.Application.Services.Pricing;

public sealed class PriceCandidateBuilder : IPriceCandidateBuilder
{
	public async Task<IReadOnlyCollection<PriceCandidate>> Build(
		IReadOnlyCollection<PriceOffer> offers,
		string targetStorageCode,
		CancellationToken cancellationToken = default)
	{
		var result = new List<PriceCandidate>();

		foreach (var offer in offers)
		{
			if (offer.AvailableQuantity <= 0)
				continue;
			var sourceType = offer.Source.GetSourceType();
			result.Add(
				new PriceCandidate(
					offer.Id,
					offer.ProductId,
					targetStorageCode,
					sourceType,
					offer.PurchasePrice,
					offer.CurrencyId,
					offer.AvailableQuantity,
					sourceType == PriceOfferSourceType.OurWarehouse
						? FulfillmentRouteInfo.SameStorage(targetStorageCode)
						: FulfillmentRouteInfo.FromSupplier(offer)));
		}

		return result;
	}
}
