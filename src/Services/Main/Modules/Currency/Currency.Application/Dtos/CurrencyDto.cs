using System.Text.Json.Serialization;

namespace Main.Modules.Currency.Application.Dtos;

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
}
