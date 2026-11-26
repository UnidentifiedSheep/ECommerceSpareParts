using System.Text.Json.Serialization;
using Integrations.Armtek.Converters;

namespace Integrations.Armtek.Models;

public record AssortmentItem
{
	[JsonPropertyName("PIN")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string? Number { get; init; }

	[JsonPropertyName("BRAND")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string? Brand { get; init; }

	[JsonPropertyName("NAME")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string? Name { get; init; }
}
