using FluentValidation;
using Localization.Domain.Extensions;
using Main.Application.Handlers.Producers.BaseValidators;

namespace Main.Application.Handlers.Producers.EditProducer;

public class EditProducerValidation : AbstractValidator<EditProducerCommand>
{
    public EditProducerValidation()
    {
        RuleFor(x => x.Producer.Description.Value)
            .SetValidator(new ProducerDescriptionValidator())
            .When(x => x.Producer.Description.IsSet);

        RuleFor(x => x.Producer.Name.Value)
            .NotNull()
            .WithLocalizationKey("producer.name.not.empty")
            .SetValidator(new ProducerNameValidator())
            .When(x => x.Producer.Name.IsSet);
    }
}