using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;
using Main.Entities.Product;

namespace Main.Application.Handlers.Characteristics.UpsertCharacteristic;

public class UpsertCharacteristicValidation : AbstractValidator<UpsertCharacteristicCommand>
{
	public UpsertCharacteristicValidation()
	{
		RuleFor(command => command.Id)
			.GreaterThan(0)
			.When(command => command.Id.HasValue);

		RuleFor(command => command.Name)
			.Cascade(CascadeMode.Stop)
			.NotEmpty()
			.WithLocalizableError(CharacteristicNameRequiredMessage.Instance)
			.Must(name => name.Trim().Length <= 128)
			.WithLocalizableError(ArticleCharacteristicNameMaxLengthMessage.Instance)
			.Must(name => !string.IsNullOrWhiteSpace(Characteristic.NormalizeName(name)))
			.WithLocalizableError(CharacteristicNormalizedNameRequiredMessage.Instance)
			.Must(name => Characteristic.NormalizeName(name).Length <= 128)
			.WithLocalizableError(CharacteristicNormalizedNameMaxLengthMessage.Instance);
	}
}
