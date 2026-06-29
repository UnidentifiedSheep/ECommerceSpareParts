using Abstractions.Interfaces.Validators;
using FluentValidation;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.Users.RemoveEmailFromUser;

public class RemoveEmailFronUserValidation : AbstractValidator<RemoveEmailFromUserCommand>
{
    public RemoveEmailFronUserValidation(
        IEmailValidator emailValidator)
    {
        RuleFor(x => x.Email)
            .SetValidator(new EmailValidator(emailValidator));

    }
}