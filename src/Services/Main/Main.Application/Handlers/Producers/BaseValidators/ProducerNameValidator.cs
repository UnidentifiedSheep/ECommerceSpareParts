using FluentValidation;
using Application.Common.Extensions;
using Main.Entities;

namespace Main.Application.Handlers.Producers.BaseValidators;

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
