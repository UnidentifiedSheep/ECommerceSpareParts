using SchemaGeneration.Abstractions.Enums;

namespace SchemaGeneration.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class SchemaInputControlAttribute(InputControlType inputControl) : Attribute
{
	public InputControlType InputControl { get; } = inputControl;
}
