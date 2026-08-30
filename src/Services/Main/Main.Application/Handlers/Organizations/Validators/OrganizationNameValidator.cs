using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Organizations.Validators;

public class OrganizationNameValidator : AbstractValidator<string>
{
	public OrganizationNameValidator()
	{
		RuleFor(x => x)
			.NotEmpty()
			.WithLocalizationKey("organization.name.required")
			.MinimumLength(3)
			.WithLocalizationKey("organization.name.min.length")
			.MaximumLength(128)
			.WithLocalizationKey("organization.name.max.length");
	}
}
