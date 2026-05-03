using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.ProductWeight.SetProductWeight;

public class SetProductWeightDbValidation : AbstractDbValidation<SetProductWeightCommand>
{
    public override void Build(IValidationPlan plan, SetProductWeightCommand request)
    {
        plan.ValidateProductExistsId(request.ProductId);
    }
}