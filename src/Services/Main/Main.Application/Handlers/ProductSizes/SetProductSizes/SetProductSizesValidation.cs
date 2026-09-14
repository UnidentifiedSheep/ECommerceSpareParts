using FluentValidation;
using Application.Common.Extensions;
using Main.Entities;

namespace Main.Application.Handlers.ProductSizes.SetProductSizes;

public class SetProductSizesValidation : AbstractValidator<SetProductSizesCommand>
{
	public SetProductSizesValidation()
	{
		RuleFor(x => x.Height)
			.GreaterThan(0)
			.WithLocalizableError(ArticleSizeHeightMustBeGreaterThanZeroMessage.Instance)
			.PrecisionScale(
				18,
				2,
				true)
			.WithLocalizableError(ArticleSizeHeightMaxTwoDecimalsMessage.Instance);

		RuleFor(x => x.Width)
			.GreaterThan(0)
			.WithLocalizableError(ArticleSizeWidthMustBeGreaterThanZeroMessage.Instance)
			.PrecisionScale(
				18,
				2,
				true)
			.WithLocalizableError(ArticleSizeWidthMaxTwoDecimalsMessage.Instance);

		RuleFor(x => x.Length)
			.GreaterThan(0)
			.WithLocalizableError(ArticleSizeLengthMustBeGreaterThanZeroMessage.Instance)
			.PrecisionScale(
				18,
				2,
				true)
			.WithLocalizableError(ArticleSizeLengthMaxTwoDecimalsMessage.Instance);
	}
}
