using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Modules.Currency.Abstractions.Interfaces;

namespace Main.Modules.Storage.Application.Commands.StorageContents.RestoreContent;

public class RestoreContentDbValidation(ICurrencyChecks currencyChecks)
	: AbstractDbValidation<RestoreContentCommand>
{
	public override void Build(IValidationPlan plan, RestoreContentCommand request)
		=> currencyChecks.Exists(plan, request.ContentDetails.Select(x => x.CurrencyId).Distinct());
}
