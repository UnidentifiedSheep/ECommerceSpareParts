using System.Collections;
using SchemaGeneration.Abstractions.Enums;

namespace SchemaGeneration.Extensions;

internal static class SchemaTypeMapper
{
    public static SchemaValueType GetValueType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        if (type == typeof(string) || type == typeof(Guid) || type == typeof(DateTime) ||
            type == typeof(DateTimeOffset))
            return SchemaValueType.String;
        if (type == typeof(bool)) return SchemaValueType.Boolean;
        if (type.IsEnum) return SchemaValueType.Enum;
        if (IsInteger(type)) return SchemaValueType.Integer;
        if (IsNumber(type)) return SchemaValueType.Number;
        if (type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type))
            return SchemaValueType.Array;

        return SchemaValueType.Object;
    }

    private static bool IsInteger(Type type)
    {
        return type == typeof(byte) || type == typeof(short) || type == typeof(int) ||
               type == typeof(long) || type == typeof(sbyte) || type == typeof(ushort) ||
               type == typeof(uint) || type == typeof(ulong);
    }

    private static bool IsNumber(Type type)
    {
        return type == typeof(float) || type == typeof(double) || type == typeof(decimal);
    }
}
