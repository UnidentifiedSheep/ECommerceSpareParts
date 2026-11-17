using Application.Common.Validators;
using FluentValidation;

namespace Main.Modules.Cart.Application.Handlers.Cart.ChangeCartItemCount;

public class ChangeCartItemCountValidation : AbstractValidator<ChangeCartItemCountCommand>
{
	public ChangeCartItemCountValidation() => RuleFor(x => x.NewCount).SetValidator(new CountValidator());
}
