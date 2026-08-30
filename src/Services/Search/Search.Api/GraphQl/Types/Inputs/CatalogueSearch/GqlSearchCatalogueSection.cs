using HotChocolate;

namespace Search.Api.GraphQl.Types.Inputs.CatalogueSearch;

[GraphQLName("SearchCatalogueSection")]
public sealed record GqlSearchCatalogueSection<T>
{
	[GraphQLName("items")]
	[GraphQLNonNullType(false, false)]
	public required IReadOnlyCollection<GqlSearchCatalogueSectionItem<T>> Items { get; init; }

	[GraphQLName("total")]
	public required long Total { get; init; }
}
