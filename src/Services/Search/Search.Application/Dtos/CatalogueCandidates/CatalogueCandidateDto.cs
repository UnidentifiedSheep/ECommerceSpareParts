using System.Text.Json.Serialization;

namespace Search.Application.Dtos.CatalogueCandidates;

public sealed record CatalogueCandidateDto
{
	[JsonPropertyName("id")]
	public required Guid Id { get; init; }

	[JsonPropertyName("sku")]
	public required string Sku { get; init; }

	[JsonPropertyName("producerId")]
	public required int ProducerId { get; init; }

	[JsonPropertyName("names")]
	public required IReadOnlyCollection<string> Names { get; init; }

	[JsonPropertyName("highlights")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public IReadOnlyDictionary<string, IReadOnlyCollection<string>>? Highlights { get; init; }
}
