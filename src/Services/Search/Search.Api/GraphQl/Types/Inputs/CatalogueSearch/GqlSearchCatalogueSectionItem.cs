using HotChocolate;
using Search.Api.GraphQl.Types.Highlights;

namespace Search.Api.GraphQl.Types.Inputs.CatalogueSearch;

[GraphQLName("SearchCatalogueSectionItem")]
public record GqlSearchCatalogueSectionItem<T>
{
	[GraphQLName("item")]
	public required T Item { get; init; }

	[GraphQLName("highlights")]
	public required GqlHighlights? Highlights { get; init; }
}
