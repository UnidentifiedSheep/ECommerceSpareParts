using FluentValidation;
using Application.Common.Extensions;
using Main.Entities;

namespace Main.Application.Handlers.Organizations.Validators;

public class OrganizationSystemNameValidator : AbstractValidator<string>
{
	public OrganizationSystemNameValidator()
	{
		RuleFor(x => x)
			.NotEmpty()
			.WithLocalizableError(OrganizationSystemNameRequiredMessage.Instance)
			.MaximumLength(128)
			.WithLocalizableError(OrganizationSystemNameMaxLengthMessage.Instance);
	}
}
