using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;

namespace Main.Application.Handlers.BaseValidators;

public class PriceValidator : AbstractValidator<decimal>
{
	public PriceValidator()
	{
		RuleFor(x => x).GreaterThan(0).WithLocalizableError(PriceMustBePositiveMessage.Instance);

		RuleFor(x => x)
			.PrecisionScale(
				18,
				2,
				true)
			.WithLocalizableError(PriceMaxTwoDecimalPlacesMessage.Instance);
	}
}
