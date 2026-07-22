using System.Text.Json.Serialization;
using Main.Application.Dtos.Currencies;
using Main.Application.Dtos.Users;
using Main.Enums;

namespace Main.Application.Dtos.Sale;

public record SaleDto
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("buyer")]
    public required UserDto Buyer { get; init; }

    [JsonPropertyName("organizationId")]
    public required Guid OrganizationId { get; init; }

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
    public required SaleState State { get; init; }

    [JsonPropertyName("currency")]
    public required CurrencyDto Currency { get; init; }
}
