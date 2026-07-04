using System.Text.Json.Serialization;

namespace Contracts.Sale.Model;

public record SaleContentDetailEventModel
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }

    [JsonPropertyName("saleContentId")]
    public required int SaleContentId { get; init; }

    [JsonPropertyName("storageContentId")]
    public required int StorageContentId { get; init; }

    [JsonPropertyName("currencyId")]
    public required int CurrencyId { get; init; }

    [JsonPropertyName("buyPrice")]
    public required decimal BuyPrice { get; init; }

    [JsonPropertyName("buyPriceInBaseCurrency")]
    public required decimal BuyPriceInBaseCurrency { get; init; }

    [JsonPropertyName("count")]
    public required int Count { get; init; }

    [JsonPropertyName("purchaseDatetime")]
    public required DateTime PurchaseDatetime { get; init; }
}