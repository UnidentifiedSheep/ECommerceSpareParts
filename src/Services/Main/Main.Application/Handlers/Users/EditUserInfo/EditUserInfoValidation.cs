using FluentValidation;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.Users.EditUserInfo;

public class EditUserInfoValidation : AbstractValidator<EditUserInfoCommand>
{
    public EditUserInfoValidation()
    {
        RuleFor(x => x.UserInfo)
            .SetValidator(new UserInfoValidator());
    }
}