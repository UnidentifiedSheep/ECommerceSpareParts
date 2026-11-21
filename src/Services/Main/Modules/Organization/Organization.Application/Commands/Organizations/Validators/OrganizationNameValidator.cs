using Application.Common.Extensions;
using FluentValidation;
using Main.Entities.Organization;

namespace Main.Modules.Organization.Application.Commands.Organizations.Validators;

public class OrganizationNameValidator : AbstractValidator<string>
{
	public OrganizationNameValidator()
	{
		RuleFor(x => x)
			.NotEmpty()
			.WithLocalizableError(OrganizationNameRequiredMessage.Instance)
			.MinimumLength(3)
			.WithLocalizableError(OrganizationNameMinLengthMessage.Instance)
			.MaximumLength(128)
			.WithLocalizableError(OrganizationNameMaxLengthMessage.Instance);
	}
}
