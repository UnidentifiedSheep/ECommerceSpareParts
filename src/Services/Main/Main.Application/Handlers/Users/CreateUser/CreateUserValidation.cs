using Abstractions.Interfaces.Validators;
using Abstractions.Models;
using Abstractions.Models.Options;
using Abstractions.Models.Validation;
using Extensions;
using FluentValidation;
using FluentValidation.Results;
using Main.Application.Handlers.BaseValidators;
using Main.Entities.User;
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
                    .SetValidator(new EmailDtoValidator(emailValidator)))
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
        
        RuleFor(x => x.Phones)
            .ChildRules(z =>
                z.RuleForEach(x => x))
            .Custom((z, context) =>
            {
                var list = z.ToList();
                var primaryCount = 0;
                var setOfPhones = new HashSet<string>();
                foreach (var phone in list)
                {
                    setOfPhones.Add(UserPhone.ToNormalizedPhone(phone.Number));
                    if (phone.IsPrimary) primaryCount++;
                }


                if (primaryCount > 1)
                    context.AddFailure(new ValidationFailure(
                        context.PropertyPath,
                        "Validation failed")
                    {
                        ErrorCode = "user.phone.primary.count",
                        CustomState = null
                    });

                if (list.Count > setOfPhones.Count)
                    context.AddFailure(new ValidationFailure(
                        context.PropertyPath,
                        "Validation failed")
                    {
                        ErrorCode = "user.have.duplicate.phone"
                    });
                if (list.Count < phoneOptions.Value.MinPhoneCount)
                    context.AddFailure(new ValidationFailure(
                        context.PropertyPath,
                        "Validation failed")
                    {
                        ErrorCode = "user.min.phone.count",
                        CustomState = new ValidationStateData
                        {
                            ErrorMessageArguments = [phoneOptions.Value.MinPhoneCount]
                        }
                    });
                if (list.Count > phoneOptions.Value.MaxPhoneCount)
                    context.AddFailure(new ValidationFailure(
                        context.PropertyPath,
                        "Validation failed")
                    {
                        ErrorCode = "user.max.phone.count",
                        CustomState = new ValidationStateData
                        {
                            ErrorMessageArguments = [phoneOptions.Value.MaxPhoneCount]
                        }
                    });
            });

        RuleFor(x => x.UserInfo)
            .SetValidator(new UserInfoValidator());
    }
}