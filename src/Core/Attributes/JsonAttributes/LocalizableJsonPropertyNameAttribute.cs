namespace Attributes.JsonAttributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class LocalizableJsonPropertyNameAttribute(string key)
    : Attribute
{
    public string Key { get; } = key;
}