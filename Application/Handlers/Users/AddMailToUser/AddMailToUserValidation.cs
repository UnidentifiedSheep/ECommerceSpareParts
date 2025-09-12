using Core.Interfaces;
using FluentValidation;

namespace Application.Handlers.Users.AddMailToUser;

public class AddMailToUserValidation : AbstractValidator<AddMailToUserCommand>
{
    public AddMailToUserValidation(IEmailValidator emailValidator)
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Id пользователя не может быть пустым");
        RuleFor(x => x.Email).Must(emailValidator.IsValidEmail).WithMessage("Указанная почта не является валидной.");
    }
}