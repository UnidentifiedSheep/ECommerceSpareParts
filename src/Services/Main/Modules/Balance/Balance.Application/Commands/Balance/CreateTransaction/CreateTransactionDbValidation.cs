using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Modules.Currency.Abstractions.Interfaces;
using Main.Modules.Organization.Abstractions;

namespace Main.Modules.Balance.Application.Commands.Balance.CreateTransaction;

public class CreateTransactionDbValidation(IOrganizationChecks organizationChecks,
	ICurrencyChecks currencyChecks)
	: AbstractDbValidation<CreateTransactionCommand>
{
	public override void Build(IValidationPlan plan, CreateTransactionCommand request)
	{
		organizationChecks.Exists(plan, [request.ReceiverId, request.SenderId]);
		currencyChecks.Exists(plan, request.CurrencyId);
	}
}
