using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Modules.Identity.Abstractions.Interfaces;
using Main.Modules.Organization.Abstractions;

namespace Main.Modules.Purchase.Application.Commands.Purchases.CreatePurchase;

public class CreatePurchaseDbValidation(IUserChecks userChecks, IOrganizationChecks organizationChecks)
	: AbstractDbValidation<CreatePurchaseCommand>
{
	public override void Build(IValidationPlan plan, CreatePurchaseCommand request)
	{
		userChecks.Exists(plan, request.SupplierUserId);
		organizationChecks.Exists(plan, request.SupplierOrganizationId);
		organizationChecks.MemberExists(
			plan,
			request.SupplierOrganizationId,
			request.SupplierUserId);
	}
}
