using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Modules.Currency.Abstractions.Interfaces;
using Main.Modules.Identity.Abstractions.Interfaces;
using Main.Modules.Organization.Abstractions;
using Main.Modules.Product.Abstractions;
using Main.Modules.Storage.Abstractions.Interfaces;

namespace Main.Modules.Sale.Application.Handlers.Sales.CreateSale;

public class CreateSaleDbValidation(
	ICurrencyChecks currencyChecks,
	IProductChecks productChecks,
	IUserChecks userChecks,
	IOrganizationChecks organizationChecks,
	IStorageChecks storageChecks) : AbstractDbValidation<CreateSaleCommand>
{
	public override void Build(IValidationPlan plan, CreateSaleCommand request)
	{
		currencyChecks.Exists(plan, request.CurrencyId);
		productChecks.Exists(plan, request.Contents.Select(content => content.ProductId));
		userChecks.Exists(plan, request.UserId);
		organizationChecks.Exists(plan, request.OrganizationId);
		organizationChecks.MemberExists(
			plan,
			request.OrganizationId,
			request.UserId);
		storageChecks.Exists(plan, request.StorageCode);
	}
}
