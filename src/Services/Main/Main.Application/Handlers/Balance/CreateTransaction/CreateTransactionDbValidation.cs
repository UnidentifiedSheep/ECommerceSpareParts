using Application.Common.Abstractions;
using BulkValidation.Core.Enums;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Balance.CreateTransaction;

public class CreateTransactionDbValidation : AbstractDbValidation<CreateTransactionCommand >
{
    public override void Build(IValidationPlan plan, CreateTransactionCommand request)
    {
        plan.ValidateUserExistsId(Quantifier.All, request.ReceiverId, request.SenderId, request.WhoCreatedTransaction);
    }
}