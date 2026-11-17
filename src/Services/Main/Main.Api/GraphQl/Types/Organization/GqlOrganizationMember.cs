using HotChocolate;
using Main.Api.GraphQl.Types.User;
using Main.Entities.Organization.Enums.Organization;
using Main.Modules.Organization.Application.Dtos.Organizations;

namespace Main.Api.GraphQl.Types.Organization;

[GraphQLName("OrganizationMember")]
public sealed record GqlOrganizationMember(
	[property: GraphQLIgnore]
	OrganizationMemberDto MemberDto)
{
	[GraphQLName("organization")]
	public GqlOrganization Organization => new(MemberDto.OrganizationId);

	[GraphQLName("user")]
	public GqlUser User => new(MemberDto.UserId);

	[GraphQLName("role")]
	public OrganizationRole Role => MemberDto.Role;
}
