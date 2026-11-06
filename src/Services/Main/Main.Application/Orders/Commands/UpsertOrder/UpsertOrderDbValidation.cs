using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Orders.Commands.UpsertOrder;

public class UpsertOrderDbValidation : AbstractDbValidation<UpsertOrderCommand>
{
	public override void Build(IValidationPlan plan, UpsertOrderCommand request)
	{
		if (request.Order.UserId != null)
			plan.ValidateOrganizationMemberExistsPK((request.Order.OrganizationId, request.Order.UserId.Value));
		else
			plan.ValidateOrganizationExistsId(request.Order.OrganizationId);

		plan.ValidateCurrencyExistsId(request.Order.CurrencyId);
		plan.ValidateProductExistsId(request.Order.Items.Select(item => item.ProductId).Distinct());
	}
}
