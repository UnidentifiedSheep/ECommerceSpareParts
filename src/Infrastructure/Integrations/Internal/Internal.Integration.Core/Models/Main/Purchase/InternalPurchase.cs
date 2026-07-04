using System.Text.Json.Serialization;
using Internal.Integration.Core.Models.Main.User;

namespace Internal.Integration.Core.Models.Main.Purchase;

public record InternalPurchase
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("supplier")]
    public required InternalUser Supplier { get; init; }

    [JsonPropertyName("currency")]
    public required InternalCurrency Currency { get; init; }

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
    public required InternalPurchaseLogistic? Logistics { get; init; }
}