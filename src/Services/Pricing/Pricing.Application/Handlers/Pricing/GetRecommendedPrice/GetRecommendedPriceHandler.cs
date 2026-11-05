using Application.Common.Interfaces.Cqrs;
using Application.Common.Querying;
using Application.Common.Interfaces.Settings;
using Extensions;
using MediatR;
using Pricing.Application.Dtos.Price;
using Pricing.Enums;
using Pricing.Entities.Settings;

namespace Pricing.Application.Handlers.Pricing.GetRecommendedPrice;

public sealed record GetRecommendedPriceQuery(
	int ProductId,
	int CurrencyId,
	string StorageCode,
	int Quantity,
	IReadOnlyCollection<PriceOfferSource> Sources) : IQuery<GetRecommendedPriceResult>;

public sealed record GetRecommendedPriceResult(PriceRecommendationDto Item);

public sealed class GetRecommendedPriceHandler(ISender sender, ISettingsService settingsService)
	: IQueryHandler<GetRecommendedPriceQuery, GetRecommendedPriceResult>
{
	public async Task<GetRecommendedPriceResult> Handle(
		GetRecommendedPriceQuery request,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(new GetPriceOptionsForProductQuery(
			request.ProductId,
			request.CurrencyId,
			request.StorageCode,
			request.Sources,
			Pagination: null,
			SortBy: [new SortExpression("score", true)]), cancellationToken);

		var remaining = request.Quantity;
		var total = 0m;
		var now = DateTime.UtcNow;
		var items = new List<PriceRecommendationItemDto>();

		foreach (var option in result.PriceOptions
			.Where(option => option.PriceOffer.AvailableQuantity > 0 &&
				option.PriceOffer.ExpiresAt > now &&
				(option.PriceOffer.OrderTill == null || option.PriceOffer.OrderTill > now))
			.OrderByDescending(option => option.Score)
			.ThenBy(option => option.Price)
			.ThenBy(option => option.DeliveryTime)
			.ThenBy(option => option.PriceOfferId))
		{
			var quantity = Math.Min(remaining, option.PriceOffer.AvailableQuantity);
			items.Add(new PriceRecommendationItemDto(quantity, option));
			total += option.Price * quantity;
			remaining -= quantity;
			if (remaining == 0) break;
		}

		decimal? unitPrice = null;
		if (remaining == 0)
		{
			var roundingStep = (await settingsService.GetOrDefault<PricingSetting>(cancellationToken))
				.Data.PriceRoundingStep;
			unitPrice = Rounding.RoundToStep(total / request.Quantity, roundingStep);
		}

		return new GetRecommendedPriceResult(new PriceRecommendationDto
		{
			Quantity = request.Quantity,
			MissingQuantity = remaining,
			CurrencyId = request.CurrencyId,
			UnitPrice = unitPrice,
			Items = items
		});
	}
}
