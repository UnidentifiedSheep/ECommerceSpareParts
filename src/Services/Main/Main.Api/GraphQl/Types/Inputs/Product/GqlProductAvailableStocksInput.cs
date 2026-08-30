using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Product;

[GraphQLName("ProductAvailableStocksInput")]
public record GqlProductAvailableStocksInput
{
    [GraphQLName("storageCode")]
    public required string StorageCode { get; init; }
}