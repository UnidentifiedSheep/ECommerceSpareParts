using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using LinqKit;
using Main.Application.Dtos.Organizations;
using Main.Application.Dtos.Users;
using Main.Entities.Organization;
using Main.Entities.User;
using Main.Enums.Organization;

namespace Main.Application.Projections;

public sealed class OrganizationMemberDtoProjectionProvider
    : ISingletonProjectionProvider<OrganizationMember, OrganizationMemberDto>
{
    public OrganizationMemberDtoProjectionProvider(
        IProjectionProvider<User, UserDto> userProjection)
    {
        var userToDto = userProjection.Projection;

        Projection = member => new OrganizationMemberDto
        {
            OrganizationId = member.OrganizationId,
            Role = member.Role,
            User = userToDto.Invoke(member.User)
        };
    }

    public Expression<Func<OrganizationMember, OrganizationMemberDto>> Projection { get; }
}

public sealed class OrganizationDtoProjectionProvider
    : ISingletonProjectionProvider<Organization, OrganizationDto>
{
    public OrganizationDtoProjectionProvider(
        IProjectionProvider<OrganizationMember, OrganizationMemberDto> memberProjection)
    {
        var memberToDto = memberProjection.Projection;

        Projection = organization => new OrganizationDto
        {
            Id = organization.Id,
            Type = organization.Type,
            Name = organization.Name,
            SystemName = organization.SystemName,
            Owner = memberToDto.Invoke(
                organization.Members.Single(member =>
                    member.Role == OrganizationRole.Owner))
        };
    }

    public Expression<Func<Organization, OrganizationDto>> Projection { get; }
}
