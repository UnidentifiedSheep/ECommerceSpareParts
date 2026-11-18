using SchemaGeneration.Abstractions.Enums;

namespace SchemaGeneration.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class SchemaDependsOnEntityAttribute(SchemaEntity entity, string? fieldName = null) : Attribute
{
	public string EntityName { get; } = entity.ToString();

	public string? FieldName { get; } = fieldName;
}
