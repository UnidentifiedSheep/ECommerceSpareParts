using Enums.Units;
using HotChocolate;
using Main.Application.Dtos.Product;

namespace Main.Api.GraphQl.Types.Product;

[GraphQLName("ProductWeight")]
public record GqlProductWeight(
    [property: GraphQLIgnore]
    ProductWeightDto ProductWeightDto)
{
    [GraphQLName("weight")]
    public decimal Weight => ProductWeightDto.Weight;

    [GraphQLName("unit")]
    public WeightUnit Unit => ProductWeightDto.Unit;
}