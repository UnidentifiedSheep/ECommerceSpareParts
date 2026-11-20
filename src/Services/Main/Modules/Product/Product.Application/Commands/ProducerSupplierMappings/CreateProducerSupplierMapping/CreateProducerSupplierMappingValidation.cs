using Application.Common.Extensions;
using FluentValidation;
using Main.Entities.Product;

namespace Main.Modules.Product.Application.Commands.ProducerSupplierMappings.CreateProducerSupplierMapping;

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
