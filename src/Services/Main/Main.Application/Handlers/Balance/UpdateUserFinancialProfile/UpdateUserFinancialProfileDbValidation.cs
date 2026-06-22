using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Balance.UpdateUserFinancialProfile;

public class UpdateUserFinancialProfileDbValidation : AbstractDbValidation<UpdateUserFinancialProfileCommand>
{
    public override void Build(IValidationPlan plan, UpdateUserFinancialProfileCommand request)
    {
        plan.ValidateUserExistsId(request.UserId);
    }
}