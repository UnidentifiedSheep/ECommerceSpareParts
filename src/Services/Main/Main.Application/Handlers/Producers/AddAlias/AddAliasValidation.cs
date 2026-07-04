using FluentValidation;
using Main.Application.Handlers.Producers.BaseValidators;

namespace Main.Application.Handlers.Producers.AddAlias;

public class AddAliasValidation : AbstractValidator<AddAliasCommand>
{
    public AddAliasValidation()
    {
        RuleFor(x => x.Alias)
            .SetValidator(new ProducerNameValidator());
    }
}