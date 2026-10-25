using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Organization;

[GraphQLName("OrganizationMemberInput")]
public sealed record GqlOrganizationMemberInput
{
	[GraphQLName("organizationId")]
	public required Guid OrganizationId { get; init; }

	[GraphQLName("userId")]
	public required Guid UserId { get; init; }
}
