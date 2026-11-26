using System.Text.Json.Serialization;
using Integrations.Armtek.Converters;
using Integrations.Armtek.Models;

namespace Integrations.Armtek.Responses;

public sealed record SearchResponse : ArmtekResponse
{
	[JsonPropertyName("RESP")]
	[JsonConverter(typeof(TolerantListConverter<SearchItem>))]
	public IReadOnlyList<SearchItem> Items { get; init; } = [];
}
