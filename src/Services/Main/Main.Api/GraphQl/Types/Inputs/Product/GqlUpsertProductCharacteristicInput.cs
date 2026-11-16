using HotChocolate;
using Main.Modules.Product.Application.Dtos.Product;

namespace Main.Api.GraphQl.Types.Inputs.Product;

[GraphQLName("UpsertProductCharacteristicInput")]
public sealed record GqlUpsertProductCharacteristicInput
{
	[GraphQLName("productId")]
	public required int ProductId { get; init; }

	[GraphQLName("characteristicId")]
	public required int CharacteristicId { get; init; }

	[GraphQLName("value")]
	public required string Value { get; init; }

	public UpsertCharacteristicsDto ToDto()
		=> new()
		{
			ProductId = ProductId,
			CharacteristicId = CharacteristicId,
			Value = Value
		};
}
