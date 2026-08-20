namespace SchemaGeneration.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class SchemaAcceptsAttribute(params string[] accepts) : Attribute
{
    public IReadOnlyList<string> Accepts { get; } = accepts;
}
