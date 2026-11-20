using FluentValidation;
using Main.Modules.Identity.Application.Commands.BaseValidators;

namespace Main.Modules.Identity.Application.Commands.Users.EditUserInfo;

public class EditUserInfoValidation : AbstractValidator<EditUserInfoCommand>
{
	public EditUserInfoValidation() => RuleFor(x => x.UserInfo).SetValidator(new UserInfoValidator());
}
