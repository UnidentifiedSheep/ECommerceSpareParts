using System.Text.Json.Serialization;

namespace Contracts.Sale.Model;

public record SaleEventModel
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("buyerId")]
    public required Guid BuyerId { get; init; }

    [JsonPropertyName("comment")]
    public string? Comment { get; init; }

    [JsonPropertyName("saleDatetime")]
    public required DateTime SaleDatetime { get; init; }

    [JsonPropertyName("transactionId")]
    public required Guid TransactionId { get; init; }

    [JsonPropertyName("totalSum")]
    public required decimal TotalSum { get; init; }

    [JsonPropertyName("storage")]
    public required string Storage { get; init; }

    [JsonPropertyName("rowVersion")]
    public required uint RowVersion { get; init; }

    [JsonPropertyName("state")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required SaleStateEventModel State { get; init; }

    [JsonPropertyName("currencyId")]
    public required int CurrencyId { get; init; }

    [JsonPropertyName("contents")]
    public required IReadOnlyList<SaleContentEventModel> Contents { get; init; }
}