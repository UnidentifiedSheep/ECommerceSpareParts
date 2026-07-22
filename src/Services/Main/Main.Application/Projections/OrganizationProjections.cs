using System.Linq.Expressions;
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
}
