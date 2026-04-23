using System.Text.Json.Serialization;
using Main.Application.Dtos.Currencies;

namespace Main.Application.Dtos.Sale;

public record SaleDto
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }
    
    [JsonPropertyName("buyer")]
    public required UserDto Buyer { get; init; }
    
    [JsonPropertyName("comment")]
    public string? Comment { get; init; }
    
    [JsonPropertyName("saleDatetime")]
    public required DateTime SaleDatetime { get; init; }
    
    [JsonPropertyName("transactionId")]
    public required string TransactionId { get; init; }
    
    [JsonPropertyName("totalSum")]
    public required decimal TotalSum { get; init; }
    
    [JsonPropertyName("storage")]
    public required string Storage { get; init; }
    
    [JsonPropertyName("currency")]
    public required CurrencyDto Currency { get; init; }
}