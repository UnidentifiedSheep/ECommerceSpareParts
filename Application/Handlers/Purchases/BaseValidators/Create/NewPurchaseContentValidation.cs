using Application.Handlers.BaseValidators;
using Core.Dtos.Amw.Purchase;
using FluentValidation;

namespace Application.Handlers.Purchases.BaseValidators.Create;

public class NewPurchaseContentValidation: AbstractValidator<NewPurchaseContentDto>
{
    public NewPurchaseContentValidation()
    {
        RuleFor(x => x.Price)
            .SetValidator(new PriceValidator());
        RuleFor(x => x.Count)
            .SetValidator(new CountValidator());
    }
}