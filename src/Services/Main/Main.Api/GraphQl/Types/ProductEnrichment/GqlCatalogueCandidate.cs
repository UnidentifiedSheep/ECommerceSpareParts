using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types.Producer;
using Main.Api.GraphQl.Types.Product;
using Main.Application.Dtos.Product.Enrichment;

namespace Main.Api.GraphQl.Types.ProductEnrichment;

[GraphQLName("CatalogueCandidate")]
public record GqlCatalogueCandidate(
	[property: GraphQLIgnore]
	CatalogueCandidateReviewDto CatalogueCandidateDto)
{
	[GraphQLName("id")]
	[Shareable]
	public Guid Id => CatalogueCandidateDto.Id;

	[GraphQLName("producer")]
	public GqlProducer Producer => new(CatalogueCandidateDto.Producer);

	[GraphQLName("product")]
	public GqlProduct? Product =>
		CatalogueCandidateDto.Product == null ? null : new GqlProduct(CatalogueCandidateDto.Product);

	[GraphQLName("sku")]
	public string Sku => CatalogueCandidateDto.Sku;

	[GraphQLName("supplierProducts")]
	public IReadOnlyList<GqlSupplierProduct> SupplierProducts =>
		CatalogueCandidateDto.SupplierProducts.Select(z => new GqlSupplierProduct(z)).ToList();

	[GraphQLName("crosses")]
	public async Task<GqlCatalogueCandidateCrosses> GetCrossesAsync(
		ICandidateCrossesByIdDataLoader loader,
		CancellationToken cancellationToken)
	{
		var result = await loader.LoadAsync(Id, cancellationToken);

		return new GqlCatalogueCandidateCrosses(
			result?.MappedCrosses.Select(x => new GqlCatalogueCandidate(x)).ToList() ?? [],
			result?.NotMappedCrosses.Select(x => new GqlSupplierProduct(x)).ToList() ?? []);
	}
}
