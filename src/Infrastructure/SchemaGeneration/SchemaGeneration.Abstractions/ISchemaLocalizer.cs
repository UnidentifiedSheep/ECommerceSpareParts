using SchemaGeneration.Abstractions.Models;

namespace SchemaGeneration.Abstractions;

public interface ISchemaLocalizer
{
    ObjectSchema Localize(ObjectSchema schema);
}
