using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Organizations.ChangeOrganizationMemberRole;

public class ChangeOrganizationMemberRoleValidation
    : AbstractValidator<ChangeOrganizationMemberRoleCommand>
{
    public ChangeOrganizationMemberRoleValidation()
    {
        RuleFor(x => x.Role)
            .IsInEnum()
            .WithLocalizationKey("organization.member.role.invalid");
    }
}
