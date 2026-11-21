using GraphQL.Common.Attributes;
using GraphQL.Common.Types;
using HotChocolate;
using Search.Api.GraphQl.Types.Inputs.CatalogueSearch.Criteria;
using Search.Entities;
using Search.Enums;

namespace Search.Api.GraphQl.Types.Inputs.CatalogueSearch;

[GraphQLName("CatalogueSearchInput")]
public record GqlCatalogueSearchInput
{
	[GraphQLName("query")]
	public string? Query { get; init; }

	[GraphQLName("targets")]
	public required IReadOnlyCollection<SearchTarget> Targets { get; init; }

	[GraphQLName("skuModes")]
	public IReadOnlyCollection<SearchMatchType>? SkuModes { get; init; }

	[GraphQLName("nameModes")]
	public IReadOnlyCollection<SearchMatchType>? NameModes { get; init; }

	[GraphQLName("producerIds")]
	public IReadOnlyCollection<int>? ProducerIds { get; init; } = [];

	[GraphQLName("productGroupIds")]
	public IReadOnlyCollection<int>? ProductGroupIds { get; init; } = [];

	[GraphQLName("candidateMappingStatus")]
	public CandidateMappingStatus? CandidateMappingStatus { get; init; }

	[GraphQLName("pagination")]
	public required GqlPagination Pagination { get; init; }

	[GraphQLName("productSortBy")]
	[SortableBy<Product>]
	public IReadOnlyCollection<GqlSortBy>? ProductSortBy { get; init; }

	[GraphQLName("catalogueCandidateSortBy")]
	[SortableBy<CatalogueCandidate>]
	public IReadOnlyCollection<GqlSortBy>? CatalogueCandidateSortBy { get; init; }

	[GraphQLName("productCriteria")]
	public GqlProductSearchCriteriaInput? ProductCriteria { get; init; }

	[GraphQLName("includeHighlights")]
	public bool? IncludeHighlights { get; init; }
}
