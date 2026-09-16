using HotChocolate;

namespace Main.Api.GraphQl.Types.ProductEnrichment;

[GraphQLName("CatalogueCandidateCrosses")]
public sealed record GqlCatalogueCandidateCrosses(
	[property: GraphQLName("mapped")]
	IReadOnlyList<GqlCatalogueCandidate> Mapped,
	[property: GraphQLName("notMapped")]
	IReadOnlyList<GqlSupplierProduct> NotMapped);
