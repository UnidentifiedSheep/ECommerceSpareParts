using FluentValidation;
using Main.Modules.Identity.Application.Validators;
using EmailValidator = Main.Modules.Identity.Application.Handlers.BaseValidators.EmailValidator;

namespace Main.Modules.Identity.Application.Handlers.Users.RemoveEmailFromUser;

public class RemoveEmailFronUserValidation : AbstractValidator<RemoveEmailFromUserCommand>
{
	public RemoveEmailFronUserValidation(IEmailValidator emailValidator)
		=> RuleFor(x => x.Email).SetValidator(new EmailValidator(emailValidator));
}
