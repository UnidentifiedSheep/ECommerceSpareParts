using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Purchases.EditPurchase;

public class EditPurchaseDbValidation : AbstractDbValidation<EditPurchaseCommand>
{
    public override void Build(IValidationPlan plan, EditPurchaseCommand request)
    {
        plan.ValidateUserExistsId(request.UpdatedUserId);
    }
}