using FluentValidation;
using Application.Common.Extensions;
using Main.Entities;

namespace Main.Application.Handlers.Users.ChangeUserDiscount;

public class ChangeUserDiscountValidation : AbstractValidator<ChangeUserDiscountCommand>
{
	public ChangeUserDiscountValidation()
	{
		RuleFor(command => command.Discount)
			.InclusiveBetween(0, 0.99m)
			.WithLocalizableError(UserDiscountRangeMessage.Instance);
	}
}
