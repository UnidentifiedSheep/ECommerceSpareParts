using GraphQL.Common.Types;
using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Storage;

[GraphQLName("StorageContentsSearchInput")]
public sealed record GqlStorageContentsSearchInput
{
    [GraphQLName("productId")]
    public int? ProductId { get; init; }

    [GraphQLName("storageCode")]
    public string? StorageCode { get; init; }

    [GraphQLName("sortBy")]
    public IReadOnlyList<GqlSortBy>? SortBy { get; init; }

    [GraphQLName("pagination")]
    public required GqlPagination Pagination { get; init; }

    [GraphQLName("showZeroCount")]
    public required bool ShowZeroCount { get; init; }
}
