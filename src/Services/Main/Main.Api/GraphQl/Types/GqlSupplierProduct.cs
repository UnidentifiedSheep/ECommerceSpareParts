using Enums;
using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Application.Dtos.Product.Enrichment;

namespace Main.Api.GraphQl.Types;

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

	[GraphQLName("supplier")]
	public Supplier Supplier => SupplierProductDto.Supplier;

	[GraphQLName("names")]
	public IReadOnlyList<GqlSupplierProductName> Names =>
		SupplierProductDto.Names.Select(x => new GqlSupplierProductName(x)).ToList();
}
