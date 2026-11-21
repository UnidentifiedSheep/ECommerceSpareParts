using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities.Storage;
using Main.Modules.Currency.Abstractions.Interfaces;
using Main.Modules.Product.Abstractions;

namespace Main.Modules.Storage.Application.Commands.StorageContents.AddContent;

public class AddContentDbValidation(ICurrencyChecks currencyChecks, IProductChecks productChecks)
	: AbstractDbValidation<AddContentCommand>
{
	public override void Build(IValidationPlan plan, AddContentCommand request)
	{
		plan.ValidateStorageExistsCode(request.StorageCode);
		currencyChecks.Exists(plan, request.StorageContent.Select(x => x.CurrencyId));
		productChecks.Exists(plan, request.StorageContent.Select(x => x.ProductId).Distinct());
	}
}
