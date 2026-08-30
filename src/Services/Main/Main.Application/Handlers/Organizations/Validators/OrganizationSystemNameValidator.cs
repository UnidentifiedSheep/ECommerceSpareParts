using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Organizations.Validators;

public class OrganizationSystemNameValidator : AbstractValidator<string>
{
	public OrganizationSystemNameValidator()
	{
		RuleFor(x => x)
			.NotEmpty()
			.WithLocalizationKey("organization.system.name.required")
			.MaximumLength(128)
			.WithLocalizationKey("organization.system.name.max.length");
	}
}
