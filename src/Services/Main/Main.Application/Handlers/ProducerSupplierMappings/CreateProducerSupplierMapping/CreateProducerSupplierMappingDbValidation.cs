using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.ProducerSupplierMappings.CreateProducerSupplierMapping;

public class CreateProducerSupplierMappingDbValidation : AbstractDbValidation<CreateProducerSupplierMappingCommand>
{
    public override void Build(IValidationPlan plan, CreateProducerSupplierMappingCommand request)
    {
        plan.ValidateProducerExistsId(request.ProducerSupplierMapping.ProducerId);
    }
}
