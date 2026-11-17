using FluentValidation;
using Main.Modules.Product.Application.Handlers.Producers.BaseValidators;

namespace Main.Modules.Product.Application.Handlers.ProducerAliases.AddAlias;

public class AddAliasValidation : AbstractValidator<AddAliasCommand>
{
	public AddAliasValidation() => RuleFor(x => x.Alias).SetValidator(new ProducerNameValidator());
}
