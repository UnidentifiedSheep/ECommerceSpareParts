using Abstractions.Interfaces.Validators;
using Abstractions.Models;
using Abstractions.Models.Options;
using Abstractions.Models.Validation;
using Extensions;
using FluentValidation;
using FluentValidation.Results;
using Main.Application.Handlers.BaseValidators;
using Microsoft.Extensions.Options;

namespace Main.Application.Handlers.Users.CreateUser;

public class CreateUserValidation : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidation(
        IPasswordManager passwordManager,
        IEmailValidator emailValidator,
        IOptions<UserEmailOptions> emailOptions,
        IOptions<UserPhoneOptions> phoneOptions)
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
                if (list.Count < emailOptions.Value.MinEmailCount)
                    context.AddFailure(new ValidationFailure(
                        context.PropertyPath,
                        "Validation failed")
                    {
                        ErrorCode = "user.min.email.count",
                        CustomState = new ValidationStateData
                        {
                            ErrorMessageArguments = [emailOptions.Value.MinEmailCount]
                        }
                    });
                if (list.Count > emailOptions.Value.MaxEmailCount)
                    context.AddFailure(new ValidationFailure(
                        context.PropertyPath,
                        "Validation failed")
                    {
                        ErrorCode = "user.max.email.count",
                        CustomState = new ValidationStateData
                        {
                            ErrorMessageArguments = [emailOptions.Value.MaxEmailCount]
                        }
                    });
            });

        RuleFor(x => x.UserInfo)
            .SetValidator(new UserInfoValidator());
    }
}