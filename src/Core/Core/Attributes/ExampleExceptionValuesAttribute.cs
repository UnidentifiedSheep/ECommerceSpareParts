namespace Core.Attributes;

[AttributeUsage(AttributeTargets.Constructor)]
public class ExampleExceptionValuesAttribute(bool isArray = false, params object[] @params) : Attribute
{
    public bool IsArray => isArray;
    public object[] Params { get; } = @params;
}