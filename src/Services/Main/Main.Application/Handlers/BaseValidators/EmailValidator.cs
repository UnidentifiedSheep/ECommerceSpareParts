using Abstractions.Interfaces.Validators;
using FluentValidation;
using Localization.Domain.Extensions;
using Main.Application.Dtos.Emails;

namespace Main.Application.Handlers.BaseValidators;

public class EmailDtoValidator : AbstractValidator<EmailDto>
{
    public EmailDtoValidator(IEmailValidator emailValidator)
    {
        RuleFor(x => x.Email)
            .Must(emailValidator.IsValidEmail)
            .WithLocalizationKey("email.must.be.valid");
    }
}

public class EmailValidator : AbstractValidator<string>
{
    public EmailValidator(IEmailValidator emailValidator)
    {
        RuleFor(x => x)
            .Must(emailValidator.IsValidEmail)
            .WithLocalizationKey("email.must.be.valid");
    }
}