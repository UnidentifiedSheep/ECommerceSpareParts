using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;
using Main.Entities.Producer;

namespace Main.Application.Handlers.Producers.AddAlias;

public class AddAliasDbValidation : AbstractDbValidation<AddAliasCommand>
{
    public override void Build(IValidationPlan plan, AddAliasCommand request)
    {
        plan.ValidateProducerExistsId(request.ProducerId)
            .ValidateProducerAliasNotExistsAlias(Producer.ToNormalizedName(request.Alias));
    }
}