using SchemaGeneration.Abstractions;
using SchemaGeneration.Abstractions.Models;

namespace SchemaGeneration.Generators;

public sealed class LocalizedSchemaGenerator(
	ISchemaGenerator schemaGenerator,
	ISchemaLocalizer schemaLocalizer) : ISchemaGenerator
{
	public ObjectSchema Generate<T>() => schemaLocalizer.Localize(schemaGenerator.Generate<T>());

	public ObjectSchema Generate(Type type) => schemaLocalizer.Localize(schemaGenerator.Generate(type));
}
