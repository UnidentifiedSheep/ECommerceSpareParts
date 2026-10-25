using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.User;

[GraphQLName("UserStorageInput")]
public sealed record GqlUserStorageInput
{
	[GraphQLName("userId")]
	public required Guid UserId { get; init; }

	[GraphQLName("storageCode")]
	public required string StorageCode { get; init; }
}
