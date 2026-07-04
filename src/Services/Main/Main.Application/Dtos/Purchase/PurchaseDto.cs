using System.Text.Json.Serialization;
using Main.Application.Dtos.Currencies;
using Main.Application.Dtos.Users;

namespace Main.Application.Dtos.Purchase;

public record PurchaseDto
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("supplier")]
    public required UserDto Supplier { get; init; }

    [JsonPropertyName("currency")]
    public required CurrencyDto Currency { get; init; }

    [JsonPropertyName("comment")]
    public required string? Comment { get; init; }

    [JsonPropertyName("storage")]
    public required string Storage { get; init; }

    [JsonPropertyName("purchaseDatetime")]
    public required DateTime PurchaseDatetime { get; init; }

    [JsonPropertyName("transactionId")]
    public required Guid TransactionId { get; init; }

    [JsonPropertyName("totalSum")]
    public required decimal TotalSum { get; init; }

    [JsonPropertyName("logistics")]
    public required PurchaseLogisticDto? Logistics { get; init; }
}