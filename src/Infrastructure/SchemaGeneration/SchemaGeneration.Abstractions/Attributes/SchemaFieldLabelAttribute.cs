namespace SchemaGeneration.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class SchemaFieldLabelAttribute(string key) : Attribute
{
    public string Key { get; } = key;
}
