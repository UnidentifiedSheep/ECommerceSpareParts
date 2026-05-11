using FluentValidation;
using Localization.Domain.Extensions;
using Main.Application.Handlers.Producers.BaseValidators;

namespace Main.Application.Handlers.Producers.AddOtherName;

public class AddOtherNameValidation : AbstractValidator<AddOtherNameCommand>
{
    public AddOtherNameValidation()
    {
        RuleFor(x => x.OtherName)
            .SetValidator(new ProducerNameValidator());

        RuleFor(x => x.WhereUsed)
            .Must(name => name.Trim().Length <= 64)
            .WithLocalizationKey("producer.other.name.where.used.max.length")
            .Must(name => name.Trim().Length >= 2)
            .WithLocalizationKey("producer.other.name.where.used.min.length");
    }
}