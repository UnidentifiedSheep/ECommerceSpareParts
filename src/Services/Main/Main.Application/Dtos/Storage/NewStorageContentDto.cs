using System.Text.Json.Serialization;

namespace Main.Application.Dtos.Storage;

public record NewStorageContentDto
{
    [JsonPropertyName("productId")]
    public required int ProductId { get; init; }
    
    [JsonPropertyName("currencyId")]
    public required int CurrencyId { get; init; }
    
    [JsonPropertyName("buyPrice")]
    public required decimal BuyPrice { get; init; }
    
    [JsonPropertyName("buyPrice")]
    public required int Count { get; init; }
    
    [JsonPropertyName("purchaseDate")]
    public required DateTime PurchaseDate { get; init; }
}