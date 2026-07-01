namespace Attributes.JsonAttributes;

[AttributeUsage(AttributeTargets.Property)]
public class DependsOnEntityAttribute : Attribute
{
    public DependsOnEntityAttribute(string entityName, string? fieldName = null)
    {
        EntityName = entityName;
        FieldName = fieldName;
    }

    public DependsOnEntityAttribute(Type type, string? fieldName = null)
    {
        EntityName = type.Name;
        FieldName = fieldName;
    }

    public string EntityName { get; }
    public string? FieldName { get; }
}