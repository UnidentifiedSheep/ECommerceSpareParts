using SchemaGeneration.Abstractions;
using SchemaGeneration.Abstractions.Models;

namespace SchemaGeneration.Generators;

public sealed class LocalizedSchemaGenerator(
    ISchemaGenerator schemaGenerator,
    ISchemaLocalizer schemaLocalizer
) : ISchemaGenerator
{
    public ObjectSchema Generate<T>()
    {
        return schemaLocalizer.Localize(schemaGenerator.Generate<T>());
    }

    public ObjectSchema Generate(Type type)
    {
        return schemaLocalizer.Localize(schemaGenerator.Generate(type));
    }
}
