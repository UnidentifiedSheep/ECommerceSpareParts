namespace SchemaGeneration.Abstractions.Models;

public sealed record ObjectSchema
{
	public int Version { get; init; } = SchemaContractVersion.Current;

	public required IReadOnlyList<FieldSchema> Fields { get; init; }

	public CsvSchema? CsvSchema { get; init; }
}
