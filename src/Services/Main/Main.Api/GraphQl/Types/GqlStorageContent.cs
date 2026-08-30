using HotChocolate;
using Main.Api.GraphQl.Types.Product;
using Main.Application.Dtos.Storage;

namespace Main.Api.GraphQl.Types;

[GraphQLName("StorageContent")]
public sealed record GqlStorageContent(
    [property: GraphQLIgnore]
    StorageContentDto Content)
{
    [GraphQLName("id")]
    public int Id => Content.Id;

    [GraphQLName("count")]
    public int Count => Content.Count;

    [GraphQLName("buyPrice")]
    public decimal BuyPrice => Content.BuyPrice;

    [GraphQLName("purchaseDatetime")]
    public DateTime PurchaseDatetime => Content.PurchaseDatetime;
    
    [GraphQLName("rowVersion")]
    public uint RowVersion => Content.RowVersion;

    [GraphQLName("currency")]
    public GqlCurrency Currency => new(Content.Currency);

    [GraphQLName("storage")]
    public GqlStorage Storage => new(Content.StorageCode);
    
    [GraphQLName("product")]
    public GqlProduct Product => new(Content.ProductId);
}
