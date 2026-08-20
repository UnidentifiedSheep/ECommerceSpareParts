namespace SchemaGeneration.Abstractions.Models;

public sealed record CsvSchema
{
    public required IReadOnlyList<CsvColumnSchema> Columns { get; init; }
}
