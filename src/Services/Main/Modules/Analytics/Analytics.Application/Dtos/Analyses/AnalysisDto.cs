using System.Text.Json.Serialization;
using SchemaGeneration.Abstractions.Models;

namespace Main.Modules.Analytics.Application.Dtos.Analyses;

public sealed record AnalysisDto
{
	[JsonPropertyName("systemName")]
	public required string SystemName { get; init; }

	[JsonPropertyName("name")]
	public required string Name { get; init; }

	[JsonPropertyName("description")]
	public required string Description { get; init; }

	[JsonPropertyName("lrtSystemName")]
	public required string LrtSystemName { get; init; }

	[JsonPropertyName("itemsChartSystemName")]
	public required string ItemsChartSystemName { get; init; }

	[JsonPropertyName("schemaVersion")]
	public required int SchemaVersion { get; init; }

	[JsonPropertyName("parametersSchema")]
	public required ObjectSchema ParametersSchema { get; init; }

	[JsonPropertyName("summarySchema")]
	public required ObjectSchema SummarySchema { get; init; }

	[JsonPropertyName("segmentsSchema")]
	public required ObjectSchema SegmentsSchema { get; init; }

	[JsonPropertyName("dataSchema")]
	public required ObjectSchema DataSchema { get; init; }
}
