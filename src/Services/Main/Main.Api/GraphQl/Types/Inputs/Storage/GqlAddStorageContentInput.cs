using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Storage;

[GraphQLName("AddStorageContentInput")]
public sealed record GqlAddStorageContentInput
{
	[GraphQLName("storageCode")]
	public required string StorageCode { get; init; }

	[GraphQLName("items")]
	public required IReadOnlyCollection<GqlNewStorageContentInput> Items { get; init; }
}
