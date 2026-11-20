using FluentValidation;
using Main.Modules.Organization.Application.Commands.Organizations.Validators;

namespace Main.Modules.Organization.Application.Commands.Organizations.CreateOrganization;

public class CreateOrganizationValidation : AbstractValidator<CreateOrganizationCommand>
{
	public CreateOrganizationValidation()
	{
		RuleFor(x => x.Name).SetValidator(new OrganizationNameValidator());

		RuleFor(x => x.SystemName).SetValidator(new OrganizationSystemNameValidator());
	}
}
