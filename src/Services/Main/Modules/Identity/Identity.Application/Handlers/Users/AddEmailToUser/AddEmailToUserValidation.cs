using FluentValidation;
using Main.Modules.Identity.Application.Validators;
using EmailValidator = Main.Modules.Identity.Application.Handlers.BaseValidators.EmailValidator;

namespace Main.Modules.Identity.Application.Handlers.Users.AddEmailToUser;

public class AddEmailToUserValidation : AbstractValidator<AddEmailToUserCommand>
{
	public AddEmailToUserValidation(IEmailValidator emailValidator)
	{
		RuleFor(x => x.Email).SetValidator(new EmailValidator(emailValidator));

		RuleFor(x => x.EmailType).IsInEnum();
	}
}
