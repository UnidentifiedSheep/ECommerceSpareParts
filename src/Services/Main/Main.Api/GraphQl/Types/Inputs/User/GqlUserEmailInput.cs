using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.User;

[GraphQLName("UserEmailInput")]
public sealed record GqlUserEmailInput
{
	[GraphQLName("userId")]
	public required Guid UserId { get; init; }

	[GraphQLName("email")]
	public required string Email { get; init; }
}
