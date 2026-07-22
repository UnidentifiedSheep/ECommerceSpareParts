using System.Text.Json.Serialization;
using Main.Application.Dtos.Organizations;
using Main.Enums.Balances;

namespace Main.Application.Dtos.Balances;

public record TransactionDto
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("sender")]
    public required OrganizationDto Sender { get; init; }

    [JsonPropertyName("receiver")]
    public required OrganizationDto Receiver { get; init; }

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

    [JsonPropertyName("sourceType")]
    public required TransactionSourceType SourceType { get; init; }
}
