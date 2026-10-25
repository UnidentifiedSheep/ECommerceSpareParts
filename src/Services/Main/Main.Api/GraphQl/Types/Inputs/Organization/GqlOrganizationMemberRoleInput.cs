using HotChocolate;
using Main.Enums.Organization;

namespace Main.Api.GraphQl.Types.Inputs.Organization;

[GraphQLName("OrganizationMemberRoleInput")]
public sealed record GqlOrganizationMemberRoleInput
{
	[GraphQLName("organizationId")]
	public required Guid OrganizationId { get; init; }

	[GraphQLName("userId")]
	public required Guid UserId { get; init; }

	[GraphQLName("role")]
	public required OrganizationRole Role { get; init; }
}
