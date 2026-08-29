using HotChocolate;
using Main.Application.Dtos.Storage;

namespace Main.Api.GraphQl.Types;

[GraphQLName("StorageContent")]
public sealed record GqlStorageContent(
    [property: GraphQLIgnore]
    StorageContentDto Content)
{
    [GraphQLName("id")]
    public int Id => Content.Id;

    [GraphQLName("storageCode")]
    public string StorageCode => Content.StorageCode;

    [GraphQLName("productId")]
    public int ProductId => Content.ProductId;

    [GraphQLName("count")]
    public int Count => Content.Count;

    [GraphQLName("buyPrice")]
    public decimal BuyPrice => Content.BuyPrice;

    [GraphQLName("purchaseDatetime")]
    public DateTime PurchaseDatetime => Content.PurchaseDatetime;

    [GraphQLName("currency")]
    public GqlCurrency Currency => new(Content.Currency);
}
