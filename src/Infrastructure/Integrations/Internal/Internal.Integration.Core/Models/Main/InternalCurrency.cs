using System.Text.Json.Serialization;

namespace Internal.Integration.Core.Models.Main;

public record InternalCurrency
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }

    [JsonPropertyName("shortName")]
    public required string ShortName { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("currencySign")]
    public required string CurrencySign { get; init; }

    [JsonPropertyName("code")]
    public required string Code { get; init; }
}