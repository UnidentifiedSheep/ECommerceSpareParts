using System.Text.Json.Serialization;

namespace Main.Abstractions.Models;

public record StorageContentPriceDto
{
    [JsonPropertyName("productId")]
    public required int ProductId { get; init; }
    
    [JsonPropertyName("storageContentId")]
    public required int StorageContentId { get; init; }
    
    [JsonPropertyName("price")]
    public required decimal Price { get; init; }
    
    [JsonPropertyName("currentCount")]
    public required int CurrentCount { get; init; }
    
    [JsonPropertyName("currencyId")]
    public required int CurrencyId { get; init; }
    
    [JsonPropertyName("logisticsPrice")]
    public required decimal? LogisticsPrice { get; init; }
    
    [JsonPropertyName("purchaseContentId")]
    public required int? PurchaseContentId { get; init; }
    
    [JsonPropertyName("purchaseId")]
    public required Guid? PurchaseId { get; init; }
    
    [JsonPropertyName("logisticsCurrencyId")]
    public required int? LogisticsCurrencyId { get; init; }
    
    [JsonPropertyName("purchaseContentCount")]
    public required int? PurchaseContentCount { get; init; }
    
    [JsonPropertyName("purchaseDatetime")]
    public required DateTime PurchaseDatetime { get; init; }
}