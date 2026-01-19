using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Producers.AddOtherName;

public class AddOtherNameDbValidation : AbstractDbValidation<AddOtherNameCommand>
{
    public override void Build(IValidationPlan plan, AddOtherNameCommand request)
    {
        plan.ValidateProducerExistsId(request.ProducerId)
            .ValidateProducersOtherNameNotExistsPK((request.ProducerId, request.OtherName.Trim(), request.WhereUsed.Trim()));
    }
}