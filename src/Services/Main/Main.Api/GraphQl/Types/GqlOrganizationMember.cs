using HotChocolate;
using Main.Api.GraphQl.Types.User;
using Main.Application.Dtos.Organizations;
using Main.Enums.Organization;

namespace Main.Api.GraphQl.Types;

[GraphQLName("OrganizationMember")]
public sealed record GqlOrganizationMember(
	[property: GraphQLIgnore] OrganizationMemberDto MemberDto)
{
	[GraphQLName("organization")]
	public GqlOrganization Organization => new(MemberDto.OrganizationId);

	[GraphQLName("user")]
	public GqlUser User => new(MemberDto.User);

	[GraphQLName("role")]
	public OrganizationRole Role => MemberDto.Role;
}
