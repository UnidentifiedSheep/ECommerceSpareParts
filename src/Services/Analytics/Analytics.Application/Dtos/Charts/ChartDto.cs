using System.Text.Json.Serialization;
using SchemaGeneration.Abstractions.Models;

namespace Analytics.Application.Dtos.Charts;

public sealed record ChartDto
{
	[JsonPropertyName("systemName")]
	public required string SystemName { get; init; }

	[JsonPropertyName("name")]
	public required string Name { get; init; }

	[JsonPropertyName("description")]
	public required string Description { get; init; }

	[JsonPropertyName("queryInputSchema")]
	public required ObjectSchema QueryInputSchema { get; init; }

	[JsonPropertyName("dataPointSchema")]
	public required ObjectSchema DataPointSchema { get; init; }
}
