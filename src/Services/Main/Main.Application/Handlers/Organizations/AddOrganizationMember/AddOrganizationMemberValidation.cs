using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Organizations.AddOrganizationMember;

public class AddOrganizationMemberValidation : AbstractValidator<AddOrganizationMemberCommand>
{
    public AddOrganizationMemberValidation()
    {
        RuleFor(x => x.Role)
            .IsInEnum()
            .WithLocalizationKey("organization.member.role.invalid");
    }
}
