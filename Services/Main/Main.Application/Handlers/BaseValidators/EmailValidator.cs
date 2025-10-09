using Core.Dtos.Emails;
using Core.Interfaces.Validators;
using FluentValidation;

namespace Main.Application.Handlers.BaseValidators;

public class EmailValidator : AbstractValidator<EmailDto>
{
    public EmailValidator(IEmailValidator emailValidator)
    {
        RuleFor(x => x.Email)
            .Must(emailValidator.IsValidEmail)
            .WithMessage("Почта не является валидной");
    }
}