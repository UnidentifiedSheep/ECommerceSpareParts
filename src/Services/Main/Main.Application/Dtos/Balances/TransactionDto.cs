using System.Text.Json.Serialization;
using Main.Enums;

namespace Main.Application.Dtos.Amw.Balances;

public record TransactionDto
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }
    
    [JsonPropertyName("senderId")]
    public required Guid SenderId { get; init; }
    
    [JsonPropertyName("receiverId")]
    public required Guid ReceiverId { get; init; }
    
    [JsonPropertyName("currencyId")]
    public required int CurrencyId { get; init; }
    
    [JsonPropertyName("amount")]
    public required decimal Amount { get; init; }
    
    [JsonPropertyName("transactionDate")]
    public required DateTime TransactionDate { get; init; }
    
    [JsonPropertyName("type")]
    public required TransactionType Type { get; init; }
    
    [JsonPropertyName("status")]
    public required TransactionStatus Status { get; init; }
}