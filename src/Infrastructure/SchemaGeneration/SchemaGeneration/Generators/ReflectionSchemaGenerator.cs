using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using SchemaGeneration.Abstractions;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;
using SchemaGeneration.Abstractions.Exceptions;
using SchemaGeneration.Abstractions.Models;
using SchemaGeneration.Extensions;

namespace SchemaGeneration.Generators;

public sealed class ReflectionSchemaGenerator : ISchemaGenerator
{
	private static readonly ConcurrentDictionary<Type, ObjectSchema> Cache = new();

	private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
	{
		TypeInfoResolver = new DefaultJsonTypeInfoResolver()
	};

	public ObjectSchema Generate<T>() => Generate(typeof(T));

	public ObjectSchema Generate(Type type)
	{
		ArgumentNullException.ThrowIfNull(type);
		return Cache.GetOrAdd(type, static rootType => BuildSchema(rootType, []));
	}

	private static ObjectSchema BuildSchema(Type type, HashSet<Type> ancestors)
	{
		ancestors.Add(type);
		try
		{
			var typeInfo = SerializerOptions.GetTypeInfo(type);
			if (typeInfo.Kind != JsonTypeInfoKind.Object)
				throw new SchemaGenerationException(type, "The root schema type must be a JSON object.");

			var fields = typeInfo
				.Properties
				.Where(property =>
					property.GetAttribute<JsonIgnoreAttribute>()?.Condition is not JsonIgnoreCondition.Always)
				.Select(property => BuildFieldSchema(property, ancestors))
				.ToArray();

			return new ObjectSchema
			{
				Fields = fields,
				CsvSchema = CsvSchemaGenerator.Generate(type)
			};
		}
		finally
		{
			ancestors.Remove(type);
		}
	}

	private static FieldSchema BuildFieldSchema(JsonPropertyInfo property, HashSet<Type> ancestors)
	{
		var inputControl = property.GetAttribute<SchemaInputControlAttribute>();
		var dependency = property.GetAttribute<SchemaDependsOnEntityAttribute>();
		var type = SchemaTypeMapper.GetValueType(property.PropertyType);

		return new FieldSchema
		{
			Name = property.Name,
			Type = type,
			LabelKey = property.GetAttribute<SchemaFieldLabelAttribute>()?.Key,
			DescriptionKey = property.GetAttribute<SchemaFieldDescriptionAttribute>()?.Key,
			Required = property.IsRequired || property.GetAttribute<RequiredSchemaFieldAttribute>() is not null,
			Control = inputControl?.InputControl,
			Accepts = property.GetAttribute<SchemaAcceptsAttribute>()?.Accepts ?? [],
			Dependency = dependency is null
				? null
				: new SchemaDependency
				{
					EntityName = dependency.EntityName, FieldName = dependency.FieldName
				},
			NestedSchema = GetNestedSchema(property.PropertyType, type, ancestors)
		};
	}

	private static ObjectSchema? GetNestedSchema(
		Type propertyType,
		SchemaValueType valueType,
		HashSet<Type> ancestors)
	{
		var nestedType = valueType switch
		{
			SchemaValueType.Object => propertyType,
			SchemaValueType.Array => GetArrayElementType(propertyType),
			_ => null
		};

		if (nestedType is null ||
		    SchemaTypeMapper.GetValueType(nestedType) != SchemaValueType.Object ||
		    ancestors.Contains(nestedType) ||
		    SerializerOptions.GetTypeInfo(nestedType).Kind != JsonTypeInfoKind.Object)
			return null;

		return BuildSchema(nestedType, ancestors);
	}

	private static Type? GetArrayElementType(Type type)
	{
		if (type.IsArray) return type.GetElementType();

		return type.GetInterfaces()
			.Append(type)
			.FirstOrDefault(candidate => candidate.IsGenericType &&
			                             candidate.GetGenericTypeDefinition() == typeof(IEnumerable<>))
			?.GetGenericArguments()[0];
	}
}
