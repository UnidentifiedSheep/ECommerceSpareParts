using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using LinqKit;
using Main.Application.Dtos.Organizations;
using Main.Application.Dtos.Users;
using Main.Entities.Organization;
using Main.Entities.User;
using Main.Enums.Organization;

namespace Main.Application.Projections;

[Lifetime(Lifetime.Singleton)]
public sealed class
	OrganizationMemberDtoProjectionProvider : ProjectionProviderBase<OrganizationMember,
	OrganizationMemberDto>
{
	public OrganizationMemberDtoProjectionProvider(IProjectionProvider<User, UserDto> userProjection)
	{
		var userToDto = userProjection.Projection;

		Projection = member => new OrganizationMemberDto
		{
			OrganizationId = member.OrganizationId,
			Role = member.Role,
			User = userToDto.Invoke(member.User)
		};
	}

	public override Expression<Func<OrganizationMember, OrganizationMemberDto>> Projection { get; }
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

[Lifetime(Lifetime.Singleton)]
public sealed class
	OrganizationListItemProjectionProvider : ProjectionProviderBase<Organization, OrganizationListItemDto>
{
	public OrganizationListItemProjectionProvider(
		IProjectionProvider<OrganizationMember, OrganizationMemberDto> memberProjection)
	{
		var memberToDto = memberProjection.Projection;

		Projection = organization => new OrganizationListItemDto
		{
			Id = organization.Id,
			Type = organization.Type,
			Name = organization.Name,
			SystemName = organization.SystemName,
			IsHidden = organization.IsHidden,
			Owner = memberToDto.Invoke(
				organization.Members.Single(member => member.Role == OrganizationRole.Owner)),
			ApproximateBalanceInBaseCurrency = organization.FinancialProfile == null
				? null
				: organization.FinancialProfile.ApproximateBalance
		};
	}

	public override Expression<Func<Organization, OrganizationListItemDto>> Projection { get; }
}
