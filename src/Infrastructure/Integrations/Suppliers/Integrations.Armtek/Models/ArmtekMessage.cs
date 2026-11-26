using System.Text.Json.Serialization;
using Integrations.Armtek.Converters;

namespace Integrations.Armtek.Models;

public record ArmtekMessage
{
	[JsonPropertyName("TYPE")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string? Type { get; init; }

	[JsonPropertyName("TEXT")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string? Text { get; init; }

	[JsonPropertyName("DATE")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string? Date { get; init; }

	[JsonIgnore]
	public bool IsError => Type is "A" or "E";
}
