namespace Attributes.JsonAttributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class CsvSchemaAttribute(Type rowType) : Attribute
{
    public Type RowType { get; } = rowType;
}
