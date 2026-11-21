using FluentValidation;
using Main.Modules.Identity.Application.Commands.BaseValidators;
using Security.Core.Interfaces;

namespace Main.Modules.Identity.Application.Commands.Auth.PasswordRecovery.ResetPassword;

public class ResetPasswordValidation : AbstractValidator<ResetPasswordCommand>
{
	public ResetPasswordValidation(IPasswordManager pwdManager)
		=> RuleFor(x => x.NewPassword).SetValidator(new PasswordValidator(pwdManager));
}
