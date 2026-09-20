using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types.Producer;
using Main.Api.GraphQl.Types.Product;
using Main.Application.Dtos.Product.Enrichment;
using Main.Entities.Exceptions;

namespace Main.Api.GraphQl.Types.ProductEnrichment;

[GraphQLName("CatalogueCandidate")]
public record GqlCatalogueCandidate
{
	private readonly CatalogueCandidateReviewDto? _candidate;

	public GqlCatalogueCandidate(CatalogueCandidateReviewDto candidate)
	{
		_candidate = candidate;
		Id = _candidate.Id;
	}

	public GqlCatalogueCandidate(Guid candidateId)
	{
		Id = candidateId;
	}

	[GraphQLName("id")]
	[Shareable]
	public Guid Id { get; }

	[GraphQLName("producer")]
	public async Task<GqlProducer> Producer(
		ICatalogueCandidateByIdDataLoader loader,
		CancellationToken cancellation)
		=> new((await GetCandidateAsync(loader, cancellation)).Producer);

	[GraphQLName("product")]
	public async Task<GqlProduct?> Product(
		ICatalogueCandidateByIdDataLoader loader,
		CancellationToken cancellation)
	{
		var res = await GetCandidateAsync(loader, cancellation);
		return res.Product == null ? null : new GqlProduct(res.Product);
	}

	[GraphQLName("sku")]
	public async Task<string> Sku(
		ICatalogueCandidateByIdDataLoader loader,
		CancellationToken cancellation)
		=> (await GetCandidateAsync(loader, cancellation)).Sku;

	[GraphQLName("supplierProducts")]
	public async Task<IReadOnlyList<GqlSupplierProduct>> SupplierProducts(
		ICatalogueCandidateByIdDataLoader loader,
		CancellationToken cancellation)
		=> (await GetCandidateAsync(loader, cancellation))
			.SupplierProducts
			.Select(z => new GqlSupplierProduct(z))
			.ToList();

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

	private async Task<CatalogueCandidateReviewDto> GetCandidateAsync(
		ICatalogueCandidateByIdDataLoader loader,
		CancellationToken cancellation) => _candidate ?? await loader.LoadAsync(Id, cancellation) ??
		throw new CatalogueCandidateNotFoundException();
}
