using Application.Common.Abstractions;
using Main.Entities.Sale;
using BulkValidation.Core.Interfaces;

using Main.Modules.Currency.Abstractions;
using Main.Modules.Identity.Abstractions;
using Main.Modules.Organization.Abstractions;
using Main.Modules.Product.Abstractions;
using Main.Modules.Storage.Abstractions;

namespace Main.Application.Handlers.Sales.CreateSale;

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
		organizationChecks.MemberExists(plan, request.OrganizationId, request.UserId);
		storageChecks.Exists(plan, request.StorageCode);
	}
}
