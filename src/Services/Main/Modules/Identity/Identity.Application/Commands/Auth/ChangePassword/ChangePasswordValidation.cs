using FluentValidation;
using Main.Modules.Identity.Application.Commands.BaseValidators;
using Security.Core.Interfaces;

namespace Main.Modules.Identity.Application.Commands.Auth.ChangePassword;

public class ChangePasswordValidation : AbstractValidator<ChangePasswordCommand>
{
	public ChangePasswordValidation(IPasswordManager passwordManager)
		=> RuleFor(x => x.NewPassword).SetValidator(new PasswordValidator(passwordManager));
}
