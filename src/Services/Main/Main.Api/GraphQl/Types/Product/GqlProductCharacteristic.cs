using HotChocolate;
using Main.Application.Dtos.Product;

namespace Main.Api.GraphQl.Types.Product;

[GraphQLName("ProductCharacteristic")]
public record GqlProductCharacteristic(
	[property: GraphQLIgnore]
	ProductCharacteristicDto CharacteristicDto)
{
	[GraphQLName("characteristic")]
	public GqlCharacteristic Characteristic => new(CharacteristicDto.Characteristic);

	[GraphQLName("value")]
	public string Value => CharacteristicDto.Value;
}
