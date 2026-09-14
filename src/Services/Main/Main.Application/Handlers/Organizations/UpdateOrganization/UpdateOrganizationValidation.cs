using FluentValidation;
using Application.Common.Extensions;
using Main.Entities;
using Main.Application.Handlers.Organizations.Validators;

namespace Main.Application.Handlers.Organizations.UpdateOrganization;

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
