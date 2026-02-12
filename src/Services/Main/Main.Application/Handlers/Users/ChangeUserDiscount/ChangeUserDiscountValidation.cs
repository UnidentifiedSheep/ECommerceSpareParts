using FluentValidation;

namespace Main.Application.Handlers.Users.ChangeUserDiscount;

public class ChangeUserDiscountValidation : AbstractValidator<ChangeUserDiscountCommand>
{
    public ChangeUserDiscountValidation()
    {
        RuleFor(command => command.DiscountRate)
            .InclusiveBetween(0, 0.99m)
            .WithMessage("Скидка должна быть от 0 до 0.99");
    }
}