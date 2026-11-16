using Application.Common.Extensions;
using Domain;
using FluentValidation;

namespace Application.Common.Validators;

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
