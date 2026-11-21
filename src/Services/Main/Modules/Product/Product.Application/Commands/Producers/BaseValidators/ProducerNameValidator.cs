using Application.Common.Extensions;
using FluentValidation;
using Main.Entities.Product;

namespace Main.Modules.Product.Application.Commands.Producers.BaseValidators;

public class ProducerNameValidator : AbstractValidator<string?>
{
	public ProducerNameValidator()
	{
		RuleFor(x => x)
			.NotEmpty()
			.WithLocalizableError(ProducerNameNotEmptyMessage.Instance)
			.Must(name => name?.Trim().Length >= 2)
			.WithLocalizableError(ProducerNameMinLengthMessage.Instance)
			.Must(name => name?.Trim().Length <= 64)
			.WithLocalizableError(ProducerNameMaxLengthMessage.Instance);
	}
}
