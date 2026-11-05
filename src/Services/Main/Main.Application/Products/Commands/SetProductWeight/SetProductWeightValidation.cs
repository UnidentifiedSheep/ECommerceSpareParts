using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;

namespace Main.Application.Products.Commands.SetProductWeight;

public class SetProductWeightValidation : AbstractValidator<SetProductWeightCommand>
{
	public SetProductWeightValidation()
	{
		RuleFor(x => x.Weight)
			.GreaterThan(0)
			.WithLocalizableError(ArticleWeightMustBeGreaterThanZeroMessage.Instance)
			.PrecisionScale(
				18,
				2,
				true)
			.WithLocalizableError(ArticleWeightMaxTwoDecimalsMessage.Instance);
	}
}
