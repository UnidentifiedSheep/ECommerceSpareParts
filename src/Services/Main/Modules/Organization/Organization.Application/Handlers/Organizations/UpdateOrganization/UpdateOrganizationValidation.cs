using Application.Common.Extensions;
using FluentValidation;
using Main.Entities.Organization;
using Main.Modules.Organization.Application.Handlers.Organizations.Validators;

namespace Main.Modules.Organization.Application.Handlers.Organizations.UpdateOrganization;

public class UpdateOrganizationValidation : AbstractValidator<UpdateOrganizationCommand>
{
	public UpdateOrganizationValidation()
	{
		RuleFor(x => x.Organization.Name.Value!)
			.NotNull()
			.WithLocalizableError(OrganizationNameRequiredMessage.Instance)
			.SetValidator(new OrganizationNameValidator())
			.When(x => x.Organization.Name.IsSet);
	}
}
