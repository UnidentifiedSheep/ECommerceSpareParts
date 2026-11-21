using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Modules.Identity.Abstractions.Interfaces;
using Security.Core.Interfaces;

namespace Main.Modules.Balance.Application.Commands.Balance.ReverseTransaction;

public class ReverseTransactionDbValidation(IUserContext userContext, IUserChecks userChecks)
	: AbstractDbValidation<ReverseTransactionCommand>
{
	public override void Build(IValidationPlan plan, ReverseTransactionCommand request)
		=> userChecks.Exists(plan, userContext.UserId);
}
