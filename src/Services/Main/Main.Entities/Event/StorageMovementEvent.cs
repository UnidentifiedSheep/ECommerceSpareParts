using System.Text.Json.Serialization;
using Main.Entities.Storage;
using Main.Enums;

namespace Main.Entities.Event;

public class StorageMovementEvent(StorageMovementEventData data) : Event<StorageMovementEventData>(data)
{
    public static StorageMovementEvent Create(StorageMovementEventData data)
    {
        return new StorageMovementEvent(data);
    }

    public static StorageMovementEvent Create(StorageContent content, StorageMovementType movementType)
    {
        var data = new StorageMovementEventData
        {
            ProductId = content.ProductId,
            StorageName = content.StorageName,
            CurrencyId = content.CurrencyId,
            Count = content.Count,
            BuyPrice = content.BuyPrice,
            MovementType = movementType
        };
        
        return new StorageMovementEvent(data);
    }
}

public record StorageMovementEventData
{
    [JsonPropertyName("productId")]
    public required int ProductId { get; init; }
    
    [JsonPropertyName("storageName")]
    public required string StorageName { get; init; }
    
    [JsonPropertyName("currencyId")]
    public required int CurrencyId { get; init; }
    
    [JsonPropertyName("count")]
    public required int Count { get; init; }
    
    [JsonPropertyName("buyPrice")]
    public required decimal BuyPrice { get; init; }
    
    [JsonPropertyName("movementType")]
    public required StorageMovementType MovementType { get; init; }
}