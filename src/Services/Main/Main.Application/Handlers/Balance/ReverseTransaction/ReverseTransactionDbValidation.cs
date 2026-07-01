using Abstractions.Interfaces;
using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Balance.ReverseTransaction;

public class ReverseTransactionDbValidation(IUserContext userContext)
    : AbstractDbValidation<ReverseTransactionCommand>
{
    public override void Build(IValidationPlan plan, ReverseTransactionCommand request)
    {
        plan.ValidateUserExistsId(userContext.UserId);
    }
}