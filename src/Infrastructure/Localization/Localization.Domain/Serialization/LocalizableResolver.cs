using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Attributes;
using Attributes.JsonAttributes;
using Localization.Abstractions.Interfaces;

namespace Localization.Domain.Serialization;

public sealed class LocalizableResolver(
    IScopedStringLocalizer localizer)
    : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var info = base.GetTypeInfo(type, options);

        if (info.Kind != JsonTypeInfoKind.Object)
            return info;

        foreach (var prop in info.Properties)
        {
            var attr = prop.AttributeProvider?
                .GetCustomAttributes(typeof(LocalizableJsonPropertyNameAttribute), true)
                .OfType<LocalizableJsonPropertyNameAttribute>()
                .FirstOrDefault();

            if (attr is null)
                continue;

            var name = localizer[attr.Key];

            if (!string.IsNullOrWhiteSpace(name))
                prop.Name = name;
        }

        return info;
    }
}