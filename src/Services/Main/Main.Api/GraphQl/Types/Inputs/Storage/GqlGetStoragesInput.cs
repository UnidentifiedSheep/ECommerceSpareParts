using GraphQL.Common.Types;
using HotChocolate;
using Main.Enums;

namespace Main.Api.GraphQl.Types.Inputs.Storage;

[GraphQLName("GetStoragesInput")]
public sealed record GqlGetStoragesInput
{
	[GraphQLName("pagination")]
	public required GqlPagination Pagination { get; init; }

	[GraphQLName("searchTerm")]
	public string? SearchTerm { get; init; }

	[GraphQLName("type")]
	public StorageType? Type { get; init; }
}
