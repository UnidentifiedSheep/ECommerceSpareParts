using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.ProductCharacteristics.AddCharacteristics;

public class AddCharacteristicsDbValidation : AbstractDbValidation<AddCharacteristicsCommand>
{
    public override void Build(IValidationPlan plan, AddCharacteristicsCommand request)
    {
        plan.ValidateProductExistsId(request.Characteristics.Select(x => x.ProductId));
    }
}