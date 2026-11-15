using Application.Common.Abstractions;
using Main.Entities.Organization;
using BulkValidation.Core.Enums;
using BulkValidation.Core.Interfaces;
using Main.Entities;
using Main.Entities.Currency;

namespace Main.Application.Handlers.Balance.CreateTransaction;

public class CreateTransactionDbValidation : AbstractDbValidation<CreateTransactionCommand>
{
	public override void Build(IValidationPlan plan, CreateTransactionCommand request)
		=> plan
			.ValidateOrganizationExistsId(
				Quantifier.All,
				request.ReceiverId,
				request.SenderId)
			.ValidateCurrencyExistsId(request.CurrencyId);
}
