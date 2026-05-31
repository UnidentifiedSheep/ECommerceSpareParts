using System.Text.Json;
using Localization.Abstractions.Interfaces;

namespace Localization.Domain.Serialization;

public sealed class ScopedLocalizedJsonSerializer(
    IScopedStringLocalizer localizer
    ) : IScopedLocalizedJsonSerializer
{
    private readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        TypeInfoResolver = new LocalizableResolver(localizer)
    };

    public string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, _options);
    }
}