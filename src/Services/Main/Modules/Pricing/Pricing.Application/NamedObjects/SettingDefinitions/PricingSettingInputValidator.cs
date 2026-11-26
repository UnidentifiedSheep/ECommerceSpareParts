using Application.Common.Extensions;
using FluentValidation;

namespace Pricing.Application.NamedObjects.SettingDefinitions;

public class PricingSettingInputValidator : AbstractValidator<PricingSettingInputData>
{
	public PricingSettingInputValidator()
	{
		RuleFor(x => x.SelectedMarkupId).GreaterThanZero();

		RuleFor(x => x.DefaultMarkup).NotNegative();

		RuleFor(x => x.OfferTtl).GreaterThanZero();

		RuleFor(x => x.PriceRoundingStep).GreaterThanZero();

		RuleFor(x => x.DeliveryDayPenalty).NotNegative();

		RuleFor(x => x.UniqProductAdditionalMarkup).GreaterThanZero();
	}
}
