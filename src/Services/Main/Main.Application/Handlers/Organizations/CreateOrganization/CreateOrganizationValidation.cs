using FluentValidation;
using Localization.Domain.Extensions;
using Main.Application.Handlers.Organizations.Validators;

namespace Main.Application.Handlers.Organizations.CreateOrganization;

public class CreateOrganizationValidation : AbstractValidator<CreateOrganizationCommand>
{
    public CreateOrganizationValidation()
    {
        RuleFor(x => x.Name)
            .SetValidator(new OrganizationNameValidator());

        RuleFor(x => x.SystemName)
            .SetValidator(new OrganizationSystemNameValidator());
    }
}
