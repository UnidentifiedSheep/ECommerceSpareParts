using System.Text.Json.Serialization;

namespace Internal.Integration.Core.Models.Main.Sale;

public record InternalSaleContentDetail
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }

    [JsonPropertyName("saleContentId")]
    public required int SaleContentId { get; init; }

    [JsonPropertyName("storageContentId")]
    public required int StorageContentId { get; init; }

    [JsonPropertyName("currency")]
    public required InternalCurrency Currency { get; init; }

    [JsonPropertyName("buyPrice")]
    public required decimal BuyPrice { get; init; }

    [JsonPropertyName("count")]
    public required int Count { get; init; }

    [JsonPropertyName("purchaseDatetime")]
    public required DateTime PurchaseDatetime { get; init; }
}