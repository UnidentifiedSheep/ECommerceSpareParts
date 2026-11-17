using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Role;

[GraphQLName("UpsertRoleInput")]
public sealed record GqlUpsertRoleInput
{
	[GraphQLName("name")]
	public required string Name { get; init; }

	[GraphQLName("description")]
	public string? Description { get; init; }
}
