using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities.Product;
using Main.Entities.Product.Producer;

namespace Main.Modules.Product.Application.Commands.ProducerSupplierMappings.CreateProducerSupplierMapping;

public class
	CreateProducerSupplierMappingDbValidation : AbstractDbValidation<CreateProducerSupplierMappingCommand>
{
	public override void Build(IValidationPlan plan, CreateProducerSupplierMappingCommand request)
		=> plan.ValidateProducerExistsId(request.ProducerSupplierMapping.ProducerId)
			.ValidateProducerSupplierMappingNotExistsSupplierName(
				(
					request.ProducerSupplierMapping.Supplier,
					Producer.ToNormalizedName(request.ProducerSupplierMapping.SupplierProducerName)));
}
