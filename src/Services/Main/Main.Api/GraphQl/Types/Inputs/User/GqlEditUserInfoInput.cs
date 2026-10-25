using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.User;

[GraphQLName("EditUserInfoInput")]
public sealed record GqlEditUserInfoInput
{
	[GraphQLName("userId")]
	public required Guid UserId { get; init; }

	[GraphQLName("userInfo")]
	public required GqlUserInfoInput UserInfo { get; init; }
}
