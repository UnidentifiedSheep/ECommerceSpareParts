using System.Text.Json.Serialization;
using Main.Application.Dtos.Currencies;

namespace Main.Application.Dtos.Sale;

public record SaleContentDetailDto
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }

    [JsonPropertyName("saleContentId")]
    public required int SaleContentId { get; init; }

    [JsonPropertyName("storageContentId")]
    public required int StorageContentId { get; init; }

    [JsonPropertyName("currency")]
    public required CurrencyDto Currency { get; init; }

    [JsonPropertyName("buyPrice")]
    public required decimal BuyPrice { get; init; }

    [JsonPropertyName("count")]
    public required int Count { get; init; }

    [JsonPropertyName("purchaseDatetime")]
    public required DateTime PurchaseDatetime { get; init; }
}