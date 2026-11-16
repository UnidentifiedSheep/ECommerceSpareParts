using HotChocolate;
using Main.Entities.Product.Enums.Products;
using Main.Modules.Product.Application.Dtos.Product;

namespace Main.Api.GraphQl.Types.Inputs.Product;

[GraphQLName("ProductLinkageInput")]
public sealed record GqlProductLinkageInput
{
	[GraphQLName("productId")]
	public required int ProductId { get; init; }

	[GraphQLName("crossProductId")]
	public required int CrossProductId { get; init; }

	[GraphQLName("linkageType")]
	public required ProductLinkageType LinkageType { get; init; }

	public NewProductLinkageDto ToDto()
		=> new()
		{
			ProductId = ProductId,
			CrossProductId = CrossProductId,
			LinkageType = LinkageType
		};
}
