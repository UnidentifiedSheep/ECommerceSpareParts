using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;
using Security.Core.Interfaces;

namespace Main.Application.Handlers.Balance.ReverseTransaction;

public class ReverseTransactionDbValidation(IUserContext userContext)
	: AbstractDbValidation<ReverseTransactionCommand>
{
	public override void Build(IValidationPlan plan, ReverseTransactionCommand request)
		=> plan.ValidateUserExistsId(userContext.UserId);
}
