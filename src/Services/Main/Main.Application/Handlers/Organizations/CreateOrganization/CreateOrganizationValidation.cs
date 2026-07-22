using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Organizations.CreateOrganization;

public class CreateOrganizationValidation : AbstractValidator<CreateOrganizationCommand>
{
    public CreateOrganizationValidation()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithLocalizationKey("organization.name.required")
            .MinimumLength(3)
            .WithLocalizationKey("organization.name.min.length")
            .MaximumLength(128)
            .WithLocalizationKey("organization.name.max.length");

        RuleFor(x => x.SystemName)
            .NotEmpty()
            .WithLocalizationKey("organization.system.name.required")
            .MaximumLength(128)
            .WithLocalizationKey("organization.system.name.max.length");
    }
}
