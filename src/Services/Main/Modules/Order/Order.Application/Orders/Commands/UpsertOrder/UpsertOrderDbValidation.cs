using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Modules.Currency.Abstractions;
using Main.Modules.Organization.Abstractions;
using Main.Modules.Product.Abstractions;

namespace Main.Application.Orders.Commands.UpsertOrder;

public class UpsertOrderDbValidation(
	IOrganizationChecks organizationChecks,
	ICurrencyChecks currencyChecks,
	IProductChecks productChecks) : AbstractDbValidation<UpsertOrderCommand>
{
	public override void Build(IValidationPlan plan, UpsertOrderCommand request)
	{
		if (request.Order.UserId != null)
			organizationChecks.MemberExists(plan, request.Order.OrganizationId, request.Order.UserId.Value);
		else
			organizationChecks.Exists(plan, request.Order.OrganizationId);

		currencyChecks.Exists(plan, request.Order.CurrencyId);
		productChecks.Exists(plan, request.Order.Items.Select(item => item.ProductId).Distinct());
	}
}
