using FluentValidation;
using Application.Common.Extensions;
using Main.Entities;

namespace Main.Application.Handlers.Organizations.Validators;

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
