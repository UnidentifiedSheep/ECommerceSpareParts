using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;
using Main.Entities.Producer;

namespace Main.Application.Handlers.Producers.AddOtherName;

public class AddOtherNameDbValidation : AbstractDbValidation<AddOtherNameCommand>
{
    public override void Build(IValidationPlan plan, AddOtherNameCommand request)
    {
        plan.ValidateProducerExistsId(request.ProducerId)
            .ValidateProducerOtherNameNotExistsOtherName(Producer.ToNormalizedName(request.OtherName));
    }
}