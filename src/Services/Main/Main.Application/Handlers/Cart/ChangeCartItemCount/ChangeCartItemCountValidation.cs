using FluentValidation;
using Main.Application.Handlers.BaseValidators;
using Application.Common.Validators;

namespace Main.Application.Handlers.Cart.ChangeCartItemCount;

public class ChangeCartItemCountValidation : AbstractValidator<ChangeCartItemCountCommand>
{
	public ChangeCartItemCountValidation() => RuleFor(x => x.NewCount).SetValidator(new CountValidator());
}
