using FluentValidation;
using Application.Common.Validators;

namespace Main.Application.Handlers.Cart.AddToCart;

public class AddToCartValidation : AbstractValidator<AddToCartCommand>
{
	public AddToCartValidation() => RuleFor(x => x.Count).SetValidator(new CountValidator());
}
