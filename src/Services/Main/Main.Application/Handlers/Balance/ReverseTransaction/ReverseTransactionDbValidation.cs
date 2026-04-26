using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Balance.DeleteTransaction;

public class ReverseTransactionDbValidation : AbstractDbValidation<ReverseTransactionCommand>
{
    public override void Build(IValidationPlan plan, ReverseTransactionCommand request)
    {
        plan.ValidateUserExistsId(request.WhoReversed);
    }
}