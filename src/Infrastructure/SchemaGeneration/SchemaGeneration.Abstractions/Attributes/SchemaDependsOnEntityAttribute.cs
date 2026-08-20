namespace SchemaGeneration.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class SchemaDependsOnEntityAttribute : Attribute
{
    public SchemaDependsOnEntityAttribute(string entityName, string? fieldName = null)
    {
        EntityName = entityName;
        FieldName = fieldName;
    }

    public SchemaDependsOnEntityAttribute(Type type, string? fieldName = null)
        : this(type.Name, fieldName)
    {
    }

    public string EntityName { get; }

    public string? FieldName { get; }
}
