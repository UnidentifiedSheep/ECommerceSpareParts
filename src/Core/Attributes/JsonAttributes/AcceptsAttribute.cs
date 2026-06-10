namespace Attributes.JsonAttributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class AcceptsAttribute(params string[] accepts) : Attribute
{
    public readonly string[] Accepts = accepts;
}