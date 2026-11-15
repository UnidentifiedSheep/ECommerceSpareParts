using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Modules.Currency.Abstractions;

namespace Main.Application.Handlers.StorageContents.EditContent;

public class EditStorageContentDbValidation(ICurrencyChecks currencyChecks)
	: AbstractDbValidation<EditStorageContentCommand>
{
	public override void Build(IValidationPlan plan, EditStorageContentCommand request)
	{
		var currencyIds = request
			.EditedFields
			.Where(x => x.Value.Model.CurrencyId.IsSet)
			.Select(x => x.Value.Model.CurrencyId.Value)
			.Distinct()
			.ToArray();

		if (currencyIds.Length != 0)
			currencyChecks.Exists(plan, currencyIds);
	}
}
