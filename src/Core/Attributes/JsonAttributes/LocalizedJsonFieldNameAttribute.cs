namespace Attributes.JsonAttributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class LocalizedJsonFieldNameAttribute(string key)
    : Attribute
{
    public string Key { get; } = key;
}