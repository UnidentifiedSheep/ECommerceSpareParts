using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Attributes.JsonAttributes;
using Localization.Abstractions.Interfaces;

namespace Localization.Domain.Serialization;

public sealed class LocalizableResolver(
    IScopedStringLocalizer localizer
)
    : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var info = base.GetTypeInfo(type, options);

        if (info.Kind != JsonTypeInfoKind.Object) return info;

        foreach (var prop in info.Properties)
        {
            var attr = GetAttribute<LocalizedJsonFieldNameAttribute>(prop);

            if (attr is null) continue;

            var name = localizer[attr.Key];

            if (!string.IsNullOrWhiteSpace(name)) prop.Name = name;
        }

        return info;
    }

    private static T? GetAttribute<T>(JsonPropertyInfo prop)
        where T : Attribute
    {
        return prop.AttributeProvider?
            .GetCustomAttributes(typeof(T), true)
            .OfType<T>()
            .FirstOrDefault();
    }
}