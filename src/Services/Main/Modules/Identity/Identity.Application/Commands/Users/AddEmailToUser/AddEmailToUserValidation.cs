using FluentValidation;
using Main.Modules.Identity.Application.Validators;
using EmailValidator = Main.Modules.Identity.Application.Commands.BaseValidators.EmailValidator;

namespace Main.Modules.Identity.Application.Commands.Users.AddEmailToUser;

public class AddEmailToUserValidation : AbstractValidator<AddEmailToUserCommand>
{
	public AddEmailToUserValidation(IEmailValidator emailValidator)
	{
		RuleFor(x => x.Email).SetValidator(new EmailValidator(emailValidator));

		RuleFor(x => x.EmailType).IsInEnum();
	}
}
