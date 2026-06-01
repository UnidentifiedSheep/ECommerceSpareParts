using Abstractions.Interfaces.Validators;
using FluentValidation;
using Mail.Interface;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.Auth.PasswordRecovery.ResetPassword;

public class ResetPasswordValidation : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordValidation(IPasswordManager pwdManager)
    {
        RuleFor(x => x.NewPassword)
            .SetValidator(new PasswordValidator(pwdManager));
    }
}