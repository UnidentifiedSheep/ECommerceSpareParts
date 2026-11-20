using Application.Common.Validators;
using FluentValidation;

namespace Main.Modules.Cart.Application.Commands.Cart.AddToCart;

public class AddToCartValidation : AbstractValidator<AddToCartCommand>
{
	public AddToCartValidation() => RuleFor(x => x.Count).SetValidator(new CountValidator());
}
