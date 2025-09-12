using FluentValidation;

namespace Application.Handlers.Users.ChangeUserDiscount;

public class ChangeUserDiscountValidation : AbstractValidator<ChangeUserDiscountCommand>
{
    public ChangeUserDiscountValidation()
    {
        RuleFor(command => command.UserId).NotEmpty().WithMessage("Айди пользователя не может быть пустым");
        RuleFor(command => command.Discount).InclusiveBetween(0, 100)
            .WithMessage("Скидка должна быть от 0 до 100");
    }
}