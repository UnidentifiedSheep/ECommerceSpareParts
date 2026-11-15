using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Modules.Identity.Abstractions;
using Security.Core.Interfaces;

namespace Main.Application.Handlers.Balance.ReverseTransaction;

public class ReverseTransactionDbValidation(IUserContext userContext, IUserChecks userChecks)
	: AbstractDbValidation<ReverseTransactionCommand>
{
	public override void Build(IValidationPlan plan, ReverseTransactionCommand request)
		=> userChecks.Exists(plan, userContext.UserId);
}
