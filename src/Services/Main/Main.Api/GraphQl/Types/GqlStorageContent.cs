using HotChocolate;
using Main.Api.GraphQl.DataLoaders;
using Main.Application.Dtos.Storage;
using Main.Entities.Exceptions;

namespace Main.Api.GraphQl.Types;

[GraphQLName("StorageContent")]
public sealed record GqlStorageContent(
    [property: GraphQLIgnore]
    StorageContentDto Content)
{
    [GraphQLName("id")]
    public int Id => Content.Id;

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

    [GraphQLName("storage")]
    public GqlStorage Storage => new(Content.StorageCode);
}
