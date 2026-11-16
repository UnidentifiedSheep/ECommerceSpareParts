using FluentValidation;
using Main.Modules.Identity.Application.Handlers.BaseValidators;
using Security.Core.Interfaces;

namespace Main.Modules.Identity.Application.Handlers.Auth.ChangePassword;

public class ChangePasswordValidation : AbstractValidator<ChangePasswordCommand>
{
	public ChangePasswordValidation(IPasswordManager passwordManager)
		=> RuleFor(x => x.NewPassword).SetValidator(new PasswordValidator(passwordManager));
}
