using FluentValidation;
using Localization.Domain.Extensions;
using Main.Application.Handlers.Producers.BaseValidators;

namespace Main.Application.Handlers.Producers.EditProducer;

public class EditProducerValidation : AbstractValidator<EditProducerCommand>
{
    public EditProducerValidation()
    {
        RuleFor(x => x.EditProducer.Description.Value)
            .SetValidator(new ProducerDescriptionValidator())
            .When(x => x.EditProducer.Description.IsSet);

        RuleFor(x => x.EditProducer.Name.Value)
            .SetValidator(new ProducerNameValidator())
            .When(x => x.EditProducer.Name.IsSet);
    }
}