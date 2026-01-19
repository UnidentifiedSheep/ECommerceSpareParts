using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Balance.DeleteTransaction;

public class DeleteTransactionDbValidation : AbstractDbValidation<DeleteTransactionCommand>
{
    public override void Build(IValidationPlan plan, DeleteTransactionCommand request)
    {
        plan.ValidateUserExistsId(request.WhoDeleteUserId);
    }
}