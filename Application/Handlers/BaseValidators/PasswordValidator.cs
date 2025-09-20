using Core.Interfaces.Validators;
using FluentValidation;

namespace Application.Handlers.BaseValidators;

public class PasswordValidator : AbstractValidator<string>
{
    public PasswordValidator(IPasswordManager passwordManager)
    {
        RuleFor(x => x)
            .Custom((password, context) =>
            {
                var (valid, errors) = passwordManager.IsPasswordMatchRules(password);
                if (valid) return;
                foreach (var error in errors)
                    context.AddFailure(error);
            });
    }
}