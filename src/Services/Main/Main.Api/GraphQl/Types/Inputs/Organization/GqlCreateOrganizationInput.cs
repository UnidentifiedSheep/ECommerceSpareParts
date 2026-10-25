using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Organization;

[GraphQLName("CreateOrganizationInput")]
public sealed record GqlCreateOrganizationInput
{
	[GraphQLName("ownerId")]
	public required Guid OwnerId { get; init; }

	[GraphQLName("name")]
	public required string Name { get; init; }

	[GraphQLName("systemName")]
	public required string SystemName { get; init; }
}
