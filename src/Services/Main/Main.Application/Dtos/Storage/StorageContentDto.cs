using System.Text.Json.Serialization;
using Main.Abstractions.Dtos.Currencies;

namespace Main.Abstractions.Dtos.Amw.Storage;

public record StorageContentDto
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }
    
    [JsonPropertyName("storageName")]
    public required string StorageName { get; init; }
    
    [JsonPropertyName("productId")]
    public required int ProductId { get; init; }
    
    [JsonPropertyName("count")]
    public required int Count { get; init; }
    
    [JsonPropertyName("buyPrice")]
    public required decimal BuyPrice { get; init; }
    
    [JsonPropertyName("purchaseDateTime")]
    public required DateTime PurchaseDatetime { get; init; }
    
    [JsonPropertyName("currency")]
    public required CurrencyDto Currency { get; init; }
}