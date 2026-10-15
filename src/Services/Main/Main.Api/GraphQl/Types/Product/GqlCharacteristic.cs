using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Application.Dtos.Product;

namespace Main.Api.GraphQl.Types.Product;

[GraphQLName("Characteristic")]
public record GqlCharacteristic(
	[property: GraphQLIgnore]
	CharacteristicDto CharacteristicDto)
{
	[GraphQLName("id")]
	[Shareable]
	public int Id => CharacteristicDto.Id;

	[GraphQLName("name")]
	public string Name => CharacteristicDto.Name;

	[GraphQLName("normalizedName")]
	public string NormalizedName => CharacteristicDto.NormalizedName;
}
