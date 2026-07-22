using System.Text.Json.Serialization;
using Internal.Integration.Core.Models.Main.Organization;
using Internal.Integration.Core.Models.Main.User;

namespace Internal.Integration.Core.Models.Main.Sale;

public record InternalSale
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("buyer")]
    public required InternalUser Buyer { get; init; }

    [JsonPropertyName("organization")]
    public required InternalOrganization Organization { get; init; }

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
    public required InternalSaleState State { get; init; }

    [JsonPropertyName("currency")]
    public required InternalCurrency Currency { get; init; }
}
