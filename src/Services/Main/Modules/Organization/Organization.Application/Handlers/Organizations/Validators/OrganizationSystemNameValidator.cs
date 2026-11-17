using Application.Common.Extensions;
using FluentValidation;
using Main.Entities.Organization;

namespace Main.Modules.Organization.Application.Handlers.Organizations.Validators;

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
