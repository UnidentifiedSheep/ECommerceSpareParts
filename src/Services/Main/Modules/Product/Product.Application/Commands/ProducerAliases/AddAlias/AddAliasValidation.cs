using FluentValidation;
using Main.Modules.Product.Application.Commands.Producers.BaseValidators;

namespace Main.Modules.Product.Application.Commands.ProducerAliases.AddAlias;

public class AddAliasValidation : AbstractValidator<AddAliasCommand>
{
	public AddAliasValidation() => RuleFor(x => x.Alias).SetValidator(new ProducerNameValidator());
}
