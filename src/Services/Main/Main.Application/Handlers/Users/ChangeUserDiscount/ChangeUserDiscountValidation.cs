using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Users.ChangeUserDiscount;

public class ChangeUserDiscountValidation : AbstractValidator<ChangeUserDiscountCommand>
{
    public ChangeUserDiscountValidation()
    {
        RuleFor(command => command.DiscountRate)
            .InclusiveBetween(0, 0.99m)
            .WithLocalizationKey("user.discount.range");
    }
}