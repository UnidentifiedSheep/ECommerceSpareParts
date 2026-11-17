using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;
using Main.Modules.Identity.Application.Dtos.Emails;
using Main.Modules.Identity.Application.Validators;

namespace Main.Modules.Identity.Application.Handlers.BaseValidators;

public class EmailDtoValidator : AbstractValidator<EmailDto>
{
	public EmailDtoValidator(IEmailValidator emailValidator)
	{
		RuleFor(x => x.Email)
			.Must(emailValidator.IsValidEmail)
			.WithLocalizableError(EmailMustBeValidMessage.Instance);
	}
}

public class EmailValidator : AbstractValidator<string>
{
	public EmailValidator(IEmailValidator emailValidator)
	{
		RuleFor(x => x)
			.Must(emailValidator.IsValidEmail)
			.WithLocalizableError(EmailMustBeValidMessage.Instance);
	}
}
