using Enums;
using HotChocolate;
using Main.Api.GraphQl.Types.Producer;
using Main.Application.Dtos.Product.References;

namespace Main.Api.GraphQl.Types.Product;

[GraphQLName("ResolvedProductReference")]
public sealed record GqlResolvedProductReference([property: GraphQLIgnore] ResolvedProductReferenceDto Reference)
{
	[GraphQLName("sku")]
	public string Sku => Reference.Sku;

	[GraphQLName("producerName")]
	public string ProducerName => Reference.ProducerName;

	[GraphQLName("supplier")]
	public Supplier? Supplier => Reference.Supplier;

	[GraphQLName("productId")]
	public int? ProductId => Reference.ProductId;

	[GraphQLName("product")]
	public GqlProduct? Product => Reference.ProductId is { } id ? new GqlProduct(id) : null;

	[GraphQLName("producer")]
	public GqlProducer? Producer
		=> Reference.ProducerId == null
			? null
			: new GqlProducer(Reference.ProducerId.Value);
}
