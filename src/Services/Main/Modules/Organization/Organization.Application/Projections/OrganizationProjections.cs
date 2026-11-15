using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using LinqKit;
using Main.Application.Dtos.Organizations;
using Main.Entities.Organization;
using Main.Enums.Organization;

namespace Main.Application.Projections;

[Lifetime(Lifetime.Singleton)]
public sealed class
	OrganizationMemberDtoProjectionProvider : ProjectionProviderBase<OrganizationMember,
	OrganizationMemberDto>
{
	public override Expression<Func<OrganizationMember, OrganizationMemberDto>> Projection { get; } =
		member => new OrganizationMemberDto
		{
			OrganizationId = member.OrganizationId,
			Role = member.Role,
			UserId = member.UserId
		};
}

[Lifetime(Lifetime.Singleton)]
public sealed class OrganizationDtoProjectionProvider : ProjectionProviderBase<Organization, OrganizationDto>
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
			IsHidden = organization.IsHidden,
			Owner = memberToDto.Invoke(
				organization.Members.Single(member => member.Role == OrganizationRole.Owner))
		};
	}

	public override Expression<Func<Organization, OrganizationDto>> Projection { get; }
}
