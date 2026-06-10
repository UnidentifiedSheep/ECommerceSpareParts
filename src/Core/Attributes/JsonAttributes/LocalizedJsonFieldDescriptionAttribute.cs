namespace Attributes.JsonAttributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class LocalizedJsonFieldDescriptionAttribute(string key) : Attribute
{
    public readonly string Key = key;
}