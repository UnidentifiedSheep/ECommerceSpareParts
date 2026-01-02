using FluentValidation;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.Cart.AddToCart;

public class AddToCartValidation : AbstractValidator<AddToCartCommand>
{
    public AddToCartValidation()
    {
        RuleFor(x => x.Count)
            .SetValidator(new CountValidator());
    }
}