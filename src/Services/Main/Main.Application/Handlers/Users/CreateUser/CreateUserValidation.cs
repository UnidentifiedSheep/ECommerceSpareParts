using Core.Extensions;
using Core.Interfaces.Validators;
using Core.Models;
using FluentValidation;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.Users.CreateUser;

public class CreateUserValidation : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidation(IPasswordManager passwordManager, IEmailValidator emailValidator, UserEmailOptions emailOptions, 
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
                    primaryCount++;
                }
                
                
                if (primaryCount > 1)
                    context.AddFailure("Не может быть больше одной основной почты");
                
                if (list.Count > setOfEmails.Count)
                    context.AddFailure("В почтах не должно быть дубликатов");
                if (list.Count < emailOptions.MinEmailCount)
                    context.AddFailure($"Минимальное количество почт у пользователя {emailOptions.MinEmailCount}");
                if (list.Count > emailOptions.MaxEmailCount)
                    context.AddFailure($"Минимальное количество почт у пользователя {emailOptions.MaxEmailCount}");
            });
        
        RuleFor(x => x.UserInfo)
            .SetValidator(new UserInfoValidator());
    }
}