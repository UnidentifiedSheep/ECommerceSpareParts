using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Sales.EditSale;

public class EditSaleDbValidation : AbstractDbValidation<EditSaleCommand>
{
    public override void Build(IValidationPlan plan, EditSaleCommand request)
    {
        plan.ValidateUserExistsId(request.UpdatedUserId);
    }
}