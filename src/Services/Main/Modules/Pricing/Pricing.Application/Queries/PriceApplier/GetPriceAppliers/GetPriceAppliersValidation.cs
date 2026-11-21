using FluentValidation;

namespace Pricing.Application.Queries.PriceApplier.GetPriceAppliers;

public class GetPriceAppliersValidation : AbstractValidator<GetPriceAppliersQuery>
{
	public GetPriceAppliersValidation() => RuleFor(x => x.Usage).IsInEnum();
}
