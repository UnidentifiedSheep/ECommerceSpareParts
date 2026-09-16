using HotChocolate;
using Main.Application.Dtos.Product;

namespace Main.Api.GraphQl.Types.Product;

[GraphQLName("ProductCharacteristic")]
public record GqlProductCharacteristic(
	[property: GraphQLIgnore]
	ProductCharacteristicDto CharacteristicDto)
{
	[GraphQLName("name")]
	public string Name => CharacteristicDto.Name;

	[GraphQLName("value")]
	public string Value => CharacteristicDto.Value;
}
