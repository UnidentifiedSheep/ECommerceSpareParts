using System.Text.Json.Serialization;

namespace Main.Application.Dtos.Currencies;

public record CurrencyDto
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

    [JsonPropertyName("toUsdRate")]
    public required decimal? ToUsdRate { get; init; }
}