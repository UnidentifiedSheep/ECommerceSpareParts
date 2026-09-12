using FluentValidation;
using Application.Common.Extensions;
using Main.Entities;

namespace Main.Application.Handlers.BaseValidators;

public class LoginValidator : AbstractValidator<string>
{
	public LoginValidator()
	{
		RuleFor(x => x)
			.NotEmpty()
			.WithLocalizableError(LoginMustNotBeEmptyMessage.Instance)
			.Must(x => x.Trim().Length >= 5)
			.WithLocalizableError(LoginMinLength5Message.Instance)
			.Must(x => x.Trim().Length <= 36)
			.WithLocalizableError(LoginMaxLength36Message.Instance)
			.Must(x => !x.Contains(' ', StringComparison.InvariantCulture))
			.WithLocalizableError(LoginCannotContainSpacesMessage.Instance)
			.Must(x => !x.Contains('@', StringComparison.InvariantCulture))
			.WithLocalizableError(LoginCannotContainAtSignMessage.Instance);
	}
}
