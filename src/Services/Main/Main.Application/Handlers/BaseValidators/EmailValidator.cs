using Core.Interfaces.Validators;
using FluentValidation;
using Main.Core.Dtos.Emails;

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