using System.Reflection;
using CsvHelper.Configuration.Attributes;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Models;
using SchemaGeneration.Extensions;

namespace SchemaGeneration.Generators;

internal static class CsvSchemaGenerator
{
	public static CsvSchema? Generate(Type type)
	{
		var schemaAttribute = type.GetCustomAttribute<CsvSchemaAttribute>();
		if (schemaAttribute is null)
			return null;

		var columns = schemaAttribute
			.RowType
			.GetProperties(BindingFlags.Public | BindingFlags.Instance)
			.Select(BuildColumnSchema)
			.ToArray();

		return new CsvSchema
		{
			Columns = columns
		};
	}

	private static CsvColumnSchema BuildColumnSchema(PropertyInfo property)
	{
		var names = GetNames(property);

		return new CsvColumnSchema
		{
			PropertyName = property.Name,
			Names = names.Count == 0 ? [property.Name] : names,
			Type = SchemaTypeMapper.GetValueType(property.PropertyType),
			Required = property.GetCustomAttribute<OptionalAttribute>() is null,
			LabelKey = property.GetCustomAttribute<SchemaFieldLabelAttribute>()?.Key,
			DescriptionKey = property.GetCustomAttribute<SchemaFieldDescriptionAttribute>()?.Key
		};
	}

	private static IReadOnlyList<string> GetNames(PropertyInfo property) =>
		property.GetCustomAttribute<NameAttribute>()?.Names ?? [];
}
