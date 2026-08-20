using System.Text.Json.Serialization.Metadata;

namespace SchemaGeneration.Extensions;

internal static class JsonPropertyInfoExtensions
{
    public static T? GetAttribute<T>(this JsonPropertyInfo property)
        where T : Attribute
    {
        return property.AttributeProvider?
            .GetCustomAttributes(typeof(T), true)
            .OfType<T>()
            .FirstOrDefault();
    }
}
