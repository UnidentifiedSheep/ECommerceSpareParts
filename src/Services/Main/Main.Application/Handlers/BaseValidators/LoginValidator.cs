using Core.Interfaces.Validators;
using FluentValidation;

namespace Main.Application.Handlers.BaseValidators;

public class LoginValidator : AbstractValidator<string>
{
    public LoginValidator()
    {
        RuleFor(x => x)
            .NotEmpty()
            .Must(x => x.Trim().Length >= 5)
            .WithMessage("Минимальная длина логина 5 символов")
            .Must(x => x.Trim().Length <= 36)
            .WithMessage("Максимальная длина логина 36 символов")
            .Must(x => !x.Contains(' '))
            .WithMessage("Логин не может содержать пробелов");
    }
}