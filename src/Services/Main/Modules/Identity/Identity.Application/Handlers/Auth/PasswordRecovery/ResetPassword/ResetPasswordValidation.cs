using FluentValidation;
using Main.Modules.Identity.Application.Handlers.BaseValidators;
using Security.Core.Interfaces;

namespace Main.Modules.Identity.Application.Handlers.Auth.PasswordRecovery.ResetPassword;

public class ResetPasswordValidation : AbstractValidator<ResetPasswordCommand>
{
	public ResetPasswordValidation(IPasswordManager pwdManager)
		=> RuleFor(x => x.NewPassword).SetValidator(new PasswordValidator(pwdManager));
}
