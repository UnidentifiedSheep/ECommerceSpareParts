using System.Text.Json.Serialization;

namespace Main.Abstractions.Dtos.Amw.Sales;

public record SaleContentDetailDto
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }

    [JsonPropertyName("saleContentId")]
    public required int SaleContentId { get; init; }

    [JsonPropertyName("storageContentId")]
    public int? StorageContentId { get; init; }

    [JsonPropertyName("storage")]
    public required string Storage { get; init; }

    [JsonPropertyName("currencyId")]
    public required int CurrencyId { get; init; }

    [JsonPropertyName("buyPrice")]
    public required decimal BuyPrice { get; init; }

    [JsonPropertyName("count")]
    public required int Count { get; init; }

    [JsonPropertyName("purchaseDatetime")]
    public required DateTime PurchaseDatetime { get; init; }
}