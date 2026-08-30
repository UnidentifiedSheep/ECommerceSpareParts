using SchemaGeneration.Abstractions.Enums;

namespace SchemaGeneration.Abstractions.Models;

public sealed record FieldSchema
{
	public required string Name { get; init; }

	public required SchemaValueType Type { get; init; }

	public string? LabelKey { get; init; }

	public string? DescriptionKey { get; init; }

	public string? Label { get; init; }

	public string? Description { get; init; }

	public bool Required { get; init; }

	public InputControlType? Control { get; init; }

	public IReadOnlyList<string> Accepts { get; init; } = [];

	public SchemaDependency? Dependency { get; init; }
}
