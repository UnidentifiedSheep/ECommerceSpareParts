namespace SchemaGeneration.Abstractions.Models;

public sealed record SchemaDependency
{
	public required string EntityName { get; init; }

	public string? FieldName { get; init; }
}
