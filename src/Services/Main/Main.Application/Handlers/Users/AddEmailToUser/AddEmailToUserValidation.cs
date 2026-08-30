using Abstractions.Interfaces.Validators;
using FluentValidation;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.Users.AddEmailToUser;

public class AddEmailToUserValidation : AbstractValidator<AddEmailToUserCommand>
{
	public AddEmailToUserValidation(IEmailValidator emailValidator)
	{
		RuleFor(x => x.Email).SetValidator(new EmailValidator(emailValidator));

		RuleFor(x => x.EmailType).IsInEnum();
	}
}
