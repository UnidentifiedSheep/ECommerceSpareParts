using SchemaGeneration.Abstractions.Models;

namespace SchemaGeneration.Abstractions;

public interface ISchemaGenerator
{
    ObjectSchema Generate<T>();

    ObjectSchema Generate(Type type);
}
