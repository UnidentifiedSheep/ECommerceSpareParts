using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;

namespace Main.Application.Handlers.Logistics.CalculateDeliveryCost;

public class CalculateDeliveryCostValidation : AbstractValidator<CalculateDeliveryCostQuery>
{
	public CalculateDeliveryCostValidation()
	{
		RuleFor(x => x.Items).NotEmpty().WithLocalizableError(DeliveryItemsNotEmptyMessage.Instance);

		RuleForEach(x => x.Items)
			.ChildRules(z =>
			{
				z
					.RuleFor(x => x.Quantity)
					.GreaterThan(0)
					.WithLocalizableError(DeliveryItemQuantityGreaterThanZeroMessage.Instance);
			});
	}
}
