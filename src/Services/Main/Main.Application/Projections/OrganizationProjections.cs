using System.Linq.Expressions;
using LinqKit;
using Main.Application.Dtos.Organizations;
using Main.Entities.Organization;

namespace Main.Application.Projections;

public static class OrganizationProjections
{
    public static readonly Expression<Func<Organization, OrganizationDto>> ToDto =
        organization => new OrganizationDto
        {
            Id = organization.Id,
            Type = organization.Type,
            Name = organization.Name,
            SystemName = organization.SystemName
        };

    public static readonly Expression<Func<OrganizationMember, OrganizationMemberDto>> MemberToDto =
        member => new OrganizationMemberDto
        {
            OrganizationId = member.OrganizationId,
            Role = member.Role,
            User = UserProjections.UserProjection.Invoke(member.User)
        };
}
