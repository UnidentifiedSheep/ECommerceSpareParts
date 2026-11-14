using FluentValidation;
using Main.Application.Handlers.BaseValidators;
using Security.Core.Interfaces;

namespace Main.Application.Handlers.Auth.ChangePassword;

public class ChangePasswordValidation : AbstractValidator<ChangePasswordCommand>
{
	public ChangePasswordValidation(IPasswordManager passwordManager)
		=> RuleFor(x => x.NewPassword).SetValidator(new PasswordValidator(passwordManager));
}
