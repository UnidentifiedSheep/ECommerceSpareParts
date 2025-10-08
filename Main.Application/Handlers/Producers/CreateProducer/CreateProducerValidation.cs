using FluentValidation;
using Main.Application.Handlers.Producers.BaseValidators;

namespace Main.Application.Handlers.Producers.CreateProducer;

public class CreateProducerValidation : AbstractValidator<CreateProducerCommand>
{
    public CreateProducerValidation()
    {
        RuleFor(x => x.NewProducer.ProducerName)
            .SetValidator(new ProducerNameValidator());

        RuleFor(x => x.NewProducer.Description)
            .SetValidator(new ProducerDescriptionValidator());
    }
}