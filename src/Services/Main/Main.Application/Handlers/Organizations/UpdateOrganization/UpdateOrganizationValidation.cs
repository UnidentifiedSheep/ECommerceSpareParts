using FluentValidation;
using Localization.Domain.Extensions;
using Main.Application.Handlers.Organizations.Validators;

namespace Main.Application.Handlers.Organizations.UpdateOrganization;

public class UpdateOrganizationValidation : AbstractValidator<UpdateOrganizationCommand>
{
	public UpdateOrganizationValidation()
	{
		RuleFor(x => x.Organization.Name.Value!)
			.NotNull()
			.WithLocalizationKey("organization.name.required")
			.SetValidator(new OrganizationNameValidator())
			.When(x => x.Organization.Name.IsSet);
	}
}
