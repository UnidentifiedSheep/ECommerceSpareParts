using Enums;
using HotChocolate;
using Main.Modules.Product.Application.Dtos.Producer.SupplierMappings;

namespace Main.Api.GraphQl.Types.Inputs.Producer;

[GraphQLName("CreateProducerSupplierMappingInput")]
public sealed record GqlCreateProducerSupplierMappingInput
{
	[GraphQLName("producerId")]
	public required int ProducerId { get; init; }

	[GraphQLName("supplier")]
	public required Supplier Supplier { get; init; }

	[GraphQLName("supplierProducerName")]
	public required string SupplierProducerName { get; init; }

	public NewProducerSupplierMapping ToDto()
		=> new()
		{
			ProducerId = ProducerId,
			Supplier = Supplier,
			SupplierProducerName = SupplierProducerName
		};
}
