using Application.Common.Extensions;
using FluentValidation;
using Main.Entities.Product;
using Main.Modules.Product.Application.Handlers.Producers.BaseValidators;

namespace Main.Modules.Product.Application.Handlers.Producers.EditProducer;

public class EditProducerValidation : AbstractValidator<EditProducerCommand>
{
	public EditProducerValidation()
	{
		RuleFor(x => x.Producer.Description.Value)
			.SetValidator(new ProducerDescriptionValidator())
			.When(x => x.Producer.Description.IsSet);

		RuleFor(x => x.Producer.Name.Value)
			.NotNull()
			.WithLocalizableError(ProducerNameNotEmptyMessage.Instance)
			.SetValidator(new ProducerNameValidator())
			.When(x => x.Producer.Name.IsSet);
	}
}
