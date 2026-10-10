using GraphQL.Common.Types;
using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Document;

[GraphQLName("SearchDocumentsInput")]
public sealed record GqlSearchDocumentsInput
{
	[GraphQLName("requesterId")]
	public Guid? RequesterId { get; init; }

	[GraphQLName("documentSystemName")]
	public string? DocumentSystemName { get; init; }

	[GraphQLName("pagination")]
	public required GqlPagination Pagination { get; init; }

	[GraphQLName("sortBy")]
	public IReadOnlyCollection<GqlSortBy>? SortBy { get; init; }
}
