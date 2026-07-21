using FluentValidation;

namespace Main.Application.Handlers.ProducerSupplierMappings.CreateProducerSupplierMapping;

public class CreateProducerSupplierMappingValidation : AbstractValidator<CreateProducerSupplierMappingCommand>
{
    public CreateProducerSupplierMappingValidation()
    {
        RuleFor(x => x.ProducerSupplierMapping.SupplierProducerName)
            .NotEmpty()
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("producer.supplier.mapping.supplier.producer.name.required");
    }
}
