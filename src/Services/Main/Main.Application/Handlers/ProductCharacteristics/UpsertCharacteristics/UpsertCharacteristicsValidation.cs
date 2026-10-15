using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;

namespace Main.Application.Handlers.ProductCharacteristics.UpsertCharacteristics;

public class UpsertCharacteristicsValidation : AbstractValidator<UpsertCharacteristicsCommand>
{
	public UpsertCharacteristicsValidation()
	{
		RuleFor(x => x.Characteristics)
			.Cascade(CascadeMode.Stop)
			.NotNull()
			.Must(items => items.Select(x => (x.ProductId, x.CharacteristicId)).Distinct().Count() == items.Count)
			.WithMessage("Each product and characteristic pair must occur only once.");

		RuleForEach(x => x.Characteristics)
			.ChildRules(z =>
			{
				z
					.RuleFor(x => x.Value)
					.Cascade(CascadeMode.Stop)
					.NotEmpty()
					.WithLocalizableError(ArticleCharacteristicValueMustNotBeEmptyMessage.Instance)
					.Must(x => x.Trim().Length >= 3)
					.WithLocalizableError(ArticleCharacteristicValueMinLengthMessage.Instance)
					.Must(x => x.Trim().Length <= 128)
					.WithLocalizableError(ArticleCharacteristicValueMaxLengthMessage.Instance);

				z
					.RuleFor(x => x.CharacteristicId)
					.GreaterThan(0);
			});
	}
}
