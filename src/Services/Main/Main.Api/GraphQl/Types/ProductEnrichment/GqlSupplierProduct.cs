using Enums;
using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.DataLoaders;
using Main.Application.Dtos.Product.Enrichment;

namespace Main.Api.GraphQl.Types.ProductEnrichment;

[GraphQLName("SupplierProduct")]
public record GqlSupplierProduct(
	[property: GraphQLIgnore]
	SupplierProductDto SupplierProductDto)
{
	[GraphQLName("id")]
	[Shareable]
	public int Id => SupplierProductDto.Id;

	[GraphQLName("sku")]
	public string Sku => SupplierProductDto.Sku;

	[GraphQLName("producer")]
	public string Producer => SupplierProductDto.Producer;

	[GraphQLName("candidateId")]
	public Guid? CandidateId => SupplierProductDto.CandidateId;

	[GraphQLName("supplier")]
	public Supplier Supplier => SupplierProductDto.Supplier;

	[GraphQLName("names")]
	public IReadOnlyList<GqlSupplierProductName> Names =>
		SupplierProductDto.Names.Select(x => new GqlSupplierProductName(x)).ToList();

	[GraphQLName("crosses")]
	public async Task<IReadOnlyList<GqlSupplierProduct>> GetCrossesAsync(
		ISupplierProductCrossesByIdDataLoader loader,
		CancellationToken cancellationToken) => (await loader.LoadAsync(Id, cancellationToken))
		?.Select(x => new GqlSupplierProduct(x))
		.ToList() ?? [];
}
