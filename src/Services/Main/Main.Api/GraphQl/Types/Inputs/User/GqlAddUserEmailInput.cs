using HotChocolate;
using Main.Enums;

namespace Main.Api.GraphQl.Types.Inputs.User;

[GraphQLName("AddUserEmailInput")]
public sealed record GqlAddUserEmailInput
{
	[GraphQLName("userId")]
	public required Guid UserId { get; init; }

	[GraphQLName("email")]
	public required string Email { get; init; }

	[GraphQLName("type")]
	public required EmailType Type { get; init; }
}
