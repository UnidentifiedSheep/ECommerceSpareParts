using FluentValidation;

namespace Pricing.Application.Handlers.PriceApplier.GetPriceAppliers;

public class GetPriceAppliersValidation : AbstractValidator<GetPriceAppliersQuery>
{
    public GetPriceAppliersValidation()
    {
        RuleFor(x => x.Usage).IsInEnum();
    }
}
