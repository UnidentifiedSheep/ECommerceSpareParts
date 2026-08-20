using Localization.Abstractions.Interfaces;
using Localization.Abstractions.Models;

namespace SchemaGeneration.Tests;

internal sealed class StubScopedStringLocalizer(
    IReadOnlyDictionary<string, string> values
) : IScopedStringLocalizer
{
    public Locale Locale { get; private set; } = "EN";

    public string this[string key] => Get(key);

    public void SetLocale(Locale locale) => Locale = locale;

    public string Get(string key) => GetOrDefault(key) ?? key;

    public string Get(string key, params object[] arguments)
    {
        return string.Format(Get(key), arguments);
    }

    public bool TryGet(string key, out string? value) => values.TryGetValue(key, out value);

    public bool TryGet(string key, out string? value, params object[] arguments)
    {
        if (!TryGet(key, out value)) return false;
        value = string.Format(value!, arguments);
        return true;
    }

    public string? GetOrDefault(string key)
    {
        return values.GetValueOrDefault(key);
    }

    public string? GetOrDefault(string key, params object[] arguments)
    {
        var value = GetOrDefault(key);
        return value is null ? null : string.Format(value, arguments);
    }

    public void Dispose()
    {
    }
}
