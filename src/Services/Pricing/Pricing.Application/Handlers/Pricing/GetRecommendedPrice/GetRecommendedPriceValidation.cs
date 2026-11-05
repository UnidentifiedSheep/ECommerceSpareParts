using Application.Common.Extensions;
using FluentValidation;
using Pricing.Entities;

namespace Pricing.Application.Handlers.Pricing.GetRecommendedPrice;

public sealed class GetRecommendedPriceValidation : AbstractValidator<GetRecommendedPriceQuery>
{
	public GetRecommendedPriceValidation()
	{
		RuleFor(x => x.ProductId)
			.GreaterThan(0)
			.WithLocalizableError(PricingProductIdMustBePositiveMessage.Instance);

		RuleFor(x => x.CurrencyId)
			.GreaterThan(0)
			.WithLocalizableError(PricingCurrencyIdMustBePositiveMessage.Instance);

		RuleFor(x => x.StorageCode)
			.NotEmpty()
			.MaximumLength(128)
			.WithLocalizableError(PricingStorageCodeInvalidMessage.Instance);

		RuleFor(x => x.Quantity)
			.GreaterThan(0)
			.WithLocalizableError(PricingQuantityMustBePositiveMessage.Instance);

		RuleFor(x => x.Sources)
			.NotNull()
			.WithLocalizableError(PricingSourcesInvalidMessage.Instance);

		RuleForEach(x => x.Sources)
			.IsInEnum()
			.WithLocalizableError(PricingSourcesInvalidMessage.Instance);
	}
}
