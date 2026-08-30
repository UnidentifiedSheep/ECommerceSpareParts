using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Application.Dtos.Product.Enrichment;

namespace Main.Api.GraphQl.Types;

[GraphQLName("SupplierProductName")]
public record GqlSupplierProductName(
	[property: GraphQLIgnore]
	SupplierProductNameDto SupplierProductName)
{
	[GraphQLName("id")]
	[Shareable]
	public int Id => SupplierProductName.Id;

	[GraphQLName("supplierProductId")]
	public int SupplierProductId => SupplierProductName.SupplierProductId;

	[GraphQLName("name")]
	public string Name => SupplierProductName.Name;
}
