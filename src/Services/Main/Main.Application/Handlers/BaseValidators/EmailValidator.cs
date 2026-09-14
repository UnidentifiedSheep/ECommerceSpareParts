using Abstractions.Interfaces.Validators;
using FluentValidation;
using Application.Common.Extensions;
using Main.Entities;
using Main.Application.Dtos.Emails;

namespace Main.Application.Handlers.BaseValidators;

public class EmailDtoValidator : AbstractValidator<EmailDto>
{
	public EmailDtoValidator(IEmailValidator emailValidator)
	{
		RuleFor(x => x.Email).Must(emailValidator.IsValidEmail).WithLocalizableError(EmailMustBeValidMessage.Instance);
	}
}

public class EmailValidator : AbstractValidator<string>
{
	public EmailValidator(IEmailValidator emailValidator)
	{
		RuleFor(x => x).Must(emailValidator.IsValidEmail).WithLocalizableError(EmailMustBeValidMessage.Instance);
	}
}
