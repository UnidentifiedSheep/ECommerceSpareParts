using FluentValidation;
using Localization.Domain.Extensions;
using Main.Application.Handlers.Producers.BaseValidators;

namespace Main.Application.Handlers.Producers.AddOtherName;

public class AddAliasValidation : AbstractValidator<AddOtherNameCommand>
{
    public AddAliasValidation()
    {
        RuleFor(x => x.Alias)
            .SetValidator(new ProducerNameValidator());
    }
}