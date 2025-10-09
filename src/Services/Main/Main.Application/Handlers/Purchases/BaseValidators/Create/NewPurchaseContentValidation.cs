using Core.Dtos.Amw.Purchase;
using FluentValidation;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.Purchases.BaseValidators.Create;

public class NewPurchaseContentValidation : AbstractValidator<NewPurchaseContentDto>
{
    public NewPurchaseContentValidation()
    {
        RuleFor(x => x.Price)
            .SetValidator(new PriceValidator());
        RuleFor(x => x.Count)
            .SetValidator(new CountValidator());
    }
}