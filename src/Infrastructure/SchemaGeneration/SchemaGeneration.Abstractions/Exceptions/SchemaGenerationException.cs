namespace SchemaGeneration.Abstractions.Exceptions;

public sealed class SchemaGenerationException(Type schemaType, string message)
	: InvalidOperationException($"Unable to generate a schema for {schemaType.FullName}: {message}")
{
	public Type SchemaType { get; } = schemaType;
}
