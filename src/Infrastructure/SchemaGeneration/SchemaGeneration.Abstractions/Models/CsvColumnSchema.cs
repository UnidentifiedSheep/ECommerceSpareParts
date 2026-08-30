using SchemaGeneration.Abstractions.Enums;

namespace SchemaGeneration.Abstractions.Models;

public sealed record CsvColumnSchema
{
	public required string PropertyName { get; init; }

	public required IReadOnlyList<string> Names { get; init; }

	public required SchemaValueType Type { get; init; }

	public bool Required { get; init; }

	public string? LabelKey { get; init; }

	public string? DescriptionKey { get; init; }

	public string? Label { get; init; }

	public string? Description { get; init; }
}
