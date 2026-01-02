using FluentValidation;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.Cart.ChangeCartItemCount;

public class ChangeCartItemCountValidation : AbstractValidator<ChangeCartItemCountCommand>
{
    public ChangeCartItemCountValidation()
    {
        RuleFor(x => x.NewCount)
            .SetValidator(new CountValidator());
    }
}