using Abstractions.Interfaces.Validators;
using Abstractions.Models;
using Abstractions.Models.Validation;
using Extensions;
using FluentValidation;
using FluentValidation.Results;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.Users.CreateUser;

public class CreateUserValidation : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidation(
        IPasswordManager passwordManager,
        IEmailValidator emailValidator,
        UserEmailOptions emailOptions,
        UserPhoneOptions phoneOptions)
    {
        RuleFor(x => x.UserName)
            .SetValidator(new LoginValidator());

        RuleFor(x => x.Password)
            .SetValidator(new PasswordValidator(passwordManager));

        RuleFor(x => x.Emails)
            .ChildRules(z =>
                z.RuleForEach(x => x)
                    .SetValidator(new EmailValidator(emailValidator)))
            .Custom((z, context) =>
            {
                var list = z.ToList();
                var primaryCount = 0;
                var setOfEmails = new HashSet<string>();
                foreach (var email in list)
                {
                    setOfEmails.Add(email.Email.ToNormalizedEmail());
                    if (email.IsPrimary) primaryCount++;
                }


                if (primaryCount > 1)
                    context.AddFailure(new ValidationFailure(
                        context.PropertyPath,
                        "Validation failed")
                    {
                        ErrorCode = "user.email.primary.count",
                        CustomState = null
                    });

                if (list.Count > setOfEmails.Count)
                    context.AddFailure(new ValidationFailure(
                        context.PropertyPath,
                        "Validation failed")
                    {
                        ErrorCode = "user.have.duplicate.email"
                    });
                if (list.Count < emailOptions.MinEmailCount)
                    context.AddFailure(new ValidationFailure(
                        context.PropertyPath,
                        "Validation failed")
                    {
                        ErrorCode = "user.min.email.count",
                        CustomState = new ValidationStateData
                        {
                            ErrorMessageArguments = [emailOptions.MinEmailCount]
                        }
                    });
                if (list.Count > emailOptions.MaxEmailCount)
                    context.AddFailure(new ValidationFailure(
                        context.PropertyPath,
                        "Validation failed")
                    {
                        ErrorCode = "user.max.email.count",
                        CustomState = new ValidationStateData
                        {
                            ErrorMessageArguments = [emailOptions.MaxEmailCount]
                        }
                    });
            });

        RuleFor(x => x.UserInfo)
            .SetValidator(new UserInfoValidator());
    }
}