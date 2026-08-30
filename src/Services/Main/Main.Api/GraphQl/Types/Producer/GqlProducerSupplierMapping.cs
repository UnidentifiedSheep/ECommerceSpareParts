using Enums;
using HotChocolate;
using Main.Application.Dtos.Producer.SupplierMappings;

namespace Main.Api.GraphQl.Types.Producer;

[GraphQLName("ProducerSupplierMapping")]
public record GqlProducerSupplierMapping(
	[property: GraphQLIgnore]
	ProducerSupplierMappingDto ProducerSupplier)
{
	[GraphQLName("id")]
	public int Id => ProducerSupplier.Id;

	[GraphQLName("supplier")]
	public Supplier Supplier => ProducerSupplier.Supplier;

	[GraphQLName("supplierProducerName")]
	public string SupplierProducerName => ProducerSupplier.SupplierProducerName;

	[GraphQLName("producer")]
	public GqlProducer Producer => new(ProducerSupplier.ProducerId);
}
