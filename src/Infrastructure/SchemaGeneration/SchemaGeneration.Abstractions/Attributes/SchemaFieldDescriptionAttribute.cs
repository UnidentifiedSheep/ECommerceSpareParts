namespace SchemaGeneration.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class SchemaFieldDescriptionAttribute(string key) : Attribute
{
    public string Key { get; } = key;
}
