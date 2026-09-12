using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;

namespace Main.Application.Handlers.ProducerSupplierMappings.CreateProducerSupplierMapping;

public class CreateProducerSupplierMappingValidation : AbstractValidator<CreateProducerSupplierMappingCommand>
{
	public CreateProducerSupplierMappingValidation()
	{
		RuleFor(x => x.ProducerSupplierMapping.SupplierProducerName)
			.NotEmpty()
			.Must(x => !string.IsNullOrWhiteSpace(x))
			.WithLocalizableError(ProducerSupplierMappingSupplierProducerNameRequiredMessage.Instance);
	}
}
