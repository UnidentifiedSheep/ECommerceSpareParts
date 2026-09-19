using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;

namespace Main.Application.Handlers.ProductCharacteristics.PatchCharacteristics;

public class PatchCharacteristicsValidation : AbstractValidator<PatchCharacteristicsCommand>
{
	public PatchCharacteristicsValidation()
	{
		RuleFor(x => x.Patch.Value.Value)
			.NotEmpty()
			.When(x => x.Patch.Value.IsSet)
			.WithLocalizableError(ArticleCharacteristicValueMustNotBeEmptyMessage.Instance);

		RuleFor(x => x.Patch.Value.Value)
			.MinimumLength(3)
			.When(x => x.Patch.Value.IsSet)
			.WithLocalizableError(ArticleCharacteristicValueMinLengthMessage.Instance);

		RuleFor(x => x.Patch.Value.Value)
			.MaximumLength(128)
			.When(x => x.Patch.Value.IsSet)
			.WithLocalizableError(ArticleCharacteristicValueMaxLengthMessage.Instance);
	}
}
