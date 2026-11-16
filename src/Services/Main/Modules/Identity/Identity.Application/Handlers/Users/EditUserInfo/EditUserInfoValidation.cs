using FluentValidation;
using Main.Modules.Identity.Application.Handlers.BaseValidators;

namespace Main.Modules.Identity.Application.Handlers.Users.EditUserInfo;

public class EditUserInfoValidation : AbstractValidator<EditUserInfoCommand>
{
	public EditUserInfoValidation() => RuleFor(x => x.UserInfo).SetValidator(new UserInfoValidator());
}
