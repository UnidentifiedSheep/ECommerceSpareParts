using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using SchemaGeneration.Abstractions;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Exceptions;
using SchemaGeneration.Abstractions.Models;
using SchemaGeneration.Extensions;

namespace SchemaGeneration.Generators;

public sealed class ReflectionSchemaGenerator : ISchemaGenerator
{
    private static readonly ConcurrentDictionary<Type, ObjectSchema> Cache = new();

    private static readonly JsonSerializerOptions SerializerOptions 
        = new(JsonSerializerDefaults.Web) 
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver() 
        };

    public ObjectSchema Generate<T>() => Generate(typeof(T));

    public ObjectSchema Generate(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return Cache.GetOrAdd(type, BuildSchema);
    }

    private static ObjectSchema BuildSchema(Type type)
    {
        var typeInfo = SerializerOptions.GetTypeInfo(type);
        if (typeInfo.Kind != JsonTypeInfoKind.Object)
            throw new SchemaGenerationException(type, "The root schema type must be a JSON object.");

        var fields = typeInfo.Properties
            .Select(BuildFieldSchema)
            .ToArray();

        return new ObjectSchema
        {
            Fields = fields,
            CsvSchema = CsvSchemaGenerator.Generate(type)
        };
    }

    private static FieldSchema BuildFieldSchema(JsonPropertyInfo property)
    {
        var inputControl = property.GetAttribute<SchemaInputControlAttribute>();
        var dependency = property.GetAttribute<SchemaDependsOnEntityAttribute>();

        return new FieldSchema
        {
            Name = property.Name,
            Type = SchemaTypeMapper.GetValueType(property.PropertyType),
            LabelKey = property.GetAttribute<SchemaFieldLabelAttribute>()?.Key,
            DescriptionKey = property.GetAttribute<SchemaFieldDescriptionAttribute>()?.Key,
            Required = property.GetAttribute<RequiredSchemaFieldAttribute>() is not null,
            Control = inputControl?.InputControl,
            Accepts = property.GetAttribute<SchemaAcceptsAttribute>()?.Accepts ?? [],
            Dependency = dependency is null
                ? null
                : new SchemaDependency
                {
                    EntityName = dependency.EntityName,
                    FieldName = dependency.FieldName
                }
        };
    }
}
