using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;
using Main.Entities.Producer;

namespace Main.Application.Handlers.Producers.AddOtherName;

public class AddAliasDbValidation : AbstractDbValidation<AddOtherNameCommand>
{
    public override void Build(IValidationPlan plan, AddOtherNameCommand request)
    {
        plan.ValidateProducerExistsId(request.ProducerId)
            .ValidateProducerAliasNotExistsAlias(Producer.ToNormalizedName(request.Alias));
    }
}