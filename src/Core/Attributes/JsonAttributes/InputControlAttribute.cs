using Enums;

namespace Attributes.JsonAttributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class InputControlAttribute(InputControlType inputControl) : Attribute
{
    public readonly InputControlType InputControl = inputControl;
}