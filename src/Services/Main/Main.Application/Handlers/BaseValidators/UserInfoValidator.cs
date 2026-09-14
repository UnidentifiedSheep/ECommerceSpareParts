using FluentValidation;
using Application.Common.Extensions;
using Main.Entities;
using Main.Application.Dtos.Users;

namespace Main.Application.Handlers.BaseValidators;

public class UserInfoValidator : AbstractValidator<UserInfoDto>
{
	public UserInfoValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty()
			.WithLocalizableError(UserNameRequiredMessage.Instance)
			.Must(x => x.Trim().Length >= 3)
			.WithLocalizableError(UserNameMinLengthMessage.Instance)
			.Must(x => !x.Any(char.IsSymbol))
			.WithLocalizableError(UserNameNoSpecialCharsMessage.Instance)
			.Must(x => x.Trim().Length <= 30)
			.WithLocalizableError(UserNameMaxLengthMessage.Instance);

		RuleFor(x => x.Surname)
			.NotEmpty()
			.WithLocalizableError(UserSurnameRequiredMessage.Instance)
			.Must(x => x.Trim().Length >= 3)
			.WithLocalizableError(UserSurnameMinLengthMessage.Instance)
			.Must(x => !x.Any(char.IsSymbol))
			.WithLocalizableError(UserSurnameNoSpecialCharsMessage.Instance)
			.Must(x => x.Trim().Length <= 30)
			.WithLocalizableError(UserSurnameMaxLengthMessage.Instance);

		RuleFor(x => x.Description)
			.Must(x => x == null || x.Trim().Length <= 300)
			.WithLocalizableError(UserDescriptionMaxLengthMessage.Instance);
	}
}
