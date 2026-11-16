using FluentValidation;
using Main.Modules.Product.Application.Handlers.Producers.BaseValidators;

namespace Main.Modules.Product.Application.Handlers.Producers.CreateProducer;

public class CreateProducerValidation : AbstractValidator<CreateProducerCommand>
{
	public CreateProducerValidation()
	{
		RuleFor(x => x.NewProducer.Name).SetValidator(new ProducerNameValidator());

		RuleFor(x => x.NewProducer.Description).SetValidator(new ProducerDescriptionValidator());
	}
}
