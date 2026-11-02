using System.Text.Json.Serialization;

namespace Main.Application.Dtos.Currencies;

public record CurrencyRateHistoryDto
{
	[JsonPropertyName("id")]
	public required int Id { get; init; }

	[JsonPropertyName("fromCurrencyId")]
	public required int FromCurrencyId { get; init; }

	[JsonPropertyName("toCurrencyId")]
	public required int ToCurrencyId { get; init; }

	[JsonPropertyName("rate")]
	public required decimal Rate { get; init; }

	[JsonPropertyName("createdAt")]
	public required DateTime CreatedAt { get; init; }
}
