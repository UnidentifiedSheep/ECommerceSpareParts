using System.Text.Json.Serialization;
using Main.Entities.Balance;
using Main.Enums;

namespace Main.Entities.Event;

public class TransactionUpdatedEvent(TransactionUpdatedEventData data) : Event<TransactionUpdatedEventData>(data)
{
    public static TransactionUpdatedEvent Create(TransactionUpdatedEventData data)
    {
        return new TransactionUpdatedEvent(data);
    }

    public static TransactionUpdatedEvent Create(Transaction transaction)
    {
        var data = new TransactionUpdatedEventData
        {
            Id = transaction.Id,
            CurrencyId = transaction.CurrencyId,
            SenderId = transaction.SenderId,
            ReceiverId = transaction.ReceiverId,
            TransactionSum = transaction.Amount,
            Type = transaction.Type,
            TransactionDatetime = transaction.TransactionDatetime,
            WhoCreated = transaction.WhoCreated,
            WhoUpdated = transaction.WhoUpdated,
            IsDeleted = transaction.IsDeleted,
            DeletedBy = transaction.ReversedBy,
            DeletedAt = transaction.ReversedAt,
            RowVersion = transaction.RowVersion,
            CreatedAt = transaction.CreatedAt,
            UpdatedAt = transaction.UpdatedAt
        };
        
        return new TransactionUpdatedEvent(data);
    }
}

public record TransactionUpdatedEventData
{
    [JsonPropertyName("transactionId")]
    public required Guid Id { get; init; }
    
    [JsonPropertyName("currencyId")]
    public required int CurrencyId { get; init; }
    
    [JsonPropertyName("senderId")]
    public required Guid SenderId { get; init; }
    
    [JsonPropertyName("receiverId")]
    public required Guid ReceiverId { get; init; }
    
    [JsonPropertyName("transactionSum")]
    public required decimal TransactionSum { get; init; }
    
    [JsonPropertyName("status")]
    [JsonConverter(typeof(JsonStringEnumConverter<TransactionType>))]
    public required TransactionType Type { get; init; }
    
    [JsonPropertyName("transactionDatetime")]
    public required DateTime TransactionDatetime { get; init; }
    
    [JsonPropertyName("createdAt")]
    public required DateTime CreatedAt { get; init; }
    
    [JsonPropertyName("updatedAt")]
    public required DateTime UpdatedAt { get; init; }
    
    [JsonPropertyName("whoCreated")]
    public required Guid WhoCreated { get; init; }
    
    [JsonPropertyName("whoUpdated")]
    public required Guid? WhoUpdated { get; init; }
    
    [JsonPropertyName("isDeleted")]
    public required bool IsDeleted { get; init; }
    
    [JsonPropertyName("deletedBy")]
    public required Guid? DeletedBy { get; init; }
    
    [JsonPropertyName("deletedAt")]
    public required DateTime? DeletedAt { get; init; }
    
    [JsonPropertyName("rowVersion")]
    public required uint RowVersion { get; init; }
}