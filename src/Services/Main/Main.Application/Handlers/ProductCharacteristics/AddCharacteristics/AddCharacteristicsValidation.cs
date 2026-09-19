using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;

namespace Main.Application.Handlers.ProductCharacteristics.AddCharacteristics;

public class AddCharacteristicsValidation : AbstractValidator<AddCharacteristicsCommand>
{
	public AddCharacteristicsValidation()
	{
		RuleForEach(x => x.Characteristics)
			.ChildRules(z =>
			{
				z
					.RuleFor(x => x.Value)
					.NotEmpty()
					.WithLocalizableError(ArticleCharacteristicValueMustNotBeEmptyMessage.Instance);

				z
					.RuleFor(x => x.Value)
					.Must(x => x.Trim().Length >= 3)
					.WithLocalizableError(ArticleCharacteristicValueMinLengthMessage.Instance);

				z
					.RuleFor(x => x.Value)
					.Must(x => x.Trim().Length <= 128)
					.WithLocalizableError(ArticleCharacteristicValueMaxLengthMessage.Instance);

				z
					.RuleFor(x => x.Name)
					.MaximumLength(128)
					.WithLocalizableError(ArticleCharacteristicNameMaxLengthMessage.Instance);
			});
	}
}
