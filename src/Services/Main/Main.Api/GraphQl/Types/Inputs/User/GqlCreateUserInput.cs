using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.User;

[GraphQLName("CreateUserInput")]
public sealed record GqlCreateUserInput
{
	[GraphQLName("userName")]
	public required string UserName { get; init; }

	[GraphQLName("password")]
	public required string Password { get; init; }

	[GraphQLName("userInfo")]
	public required GqlUserInfoInput UserInfo { get; init; }

	[GraphQLName("emails")]
	public required IReadOnlyCollection<GqlCreateUserEmailInput> Emails { get; init; }

	[GraphQLName("phones")]
	public required IReadOnlyCollection<GqlCreateUserPhoneInput> Phones { get; init; }

	[GraphQLName("roles")]
	public required IReadOnlyCollection<string> Roles { get; init; }
}
