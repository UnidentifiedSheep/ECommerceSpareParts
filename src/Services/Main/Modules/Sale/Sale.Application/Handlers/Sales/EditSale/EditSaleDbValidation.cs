using Application.Common.Abstractions;
using Main.Entities.Sale;
using BulkValidation.Core.Interfaces;

using Main.Modules.Currency.Abstractions;
using Main.Modules.Product.Abstractions;

namespace Main.Application.Handlers.Sales.EditSale;

public class EditSaleDbValidation(ICurrencyChecks currencyChecks, IProductChecks productChecks)
	: AbstractDbValidation<EditSaleCommand>
{
	public override void Build(IValidationPlan plan, EditSaleCommand request)
	{
		currencyChecks.Exists(plan, request.CurrencyId);
		productChecks.Exists(plan, request.Content.Select(x => x.ProductId));
	}
}
