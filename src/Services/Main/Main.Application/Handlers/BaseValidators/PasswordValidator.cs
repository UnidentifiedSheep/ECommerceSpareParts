using Abstractions.Interfaces.Validators;
using Abstractions.Models.Validation;
using FluentValidation;
using FluentValidation.Results;

namespace Main.Application.Handlers.BaseValidators;

public class PasswordValidator : AbstractValidator<string>
{
    public PasswordValidator(IPasswordManager passwordManager)
    {
        RuleFor(x => x)
            .Custom((password, context) =>
            {
                var (valid, errors) = passwordManager.IsPasswordMatchRules(password);
                if (valid) return;

                foreach (var (key, args) in errors)
                    context.AddFailure(new ValidationFailure(
                        context.PropertyPath,
                        "Validation failed")
                    {
                        ErrorCode = key,
                        CustomState = new ValidationStateData
                        {
                            DisplayErrorToUser = true,
                            ErrorMessageArguments = args
                        }
                    });
            });
    }
}