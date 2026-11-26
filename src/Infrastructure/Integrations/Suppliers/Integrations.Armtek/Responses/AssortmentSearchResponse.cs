using System.Text.Json.Serialization;
using Integrations.Armtek.Converters;
using Integrations.Armtek.Models;

namespace Integrations.Armtek.Responses;

public sealed record AssortmentSearchResponse : ArmtekResponse
{
	[JsonPropertyName("RESP")]
	[JsonConverter(typeof(TolerantListConverter<AssortmentItem>))]
	public IReadOnlyList<AssortmentItem> Items { get; init; } = [];
}
