using Localization.Abstractions.Interfaces;
using Localization.Abstractions.Models;

namespace Localization.Domain;

public class StringLocalizer : IStringLocalizer
{
    private readonly Dictionary<string, Dictionary<string, string>> _localization;

    public StringLocalizer(IEnumerable<ILocalizerContainer> containers)
    {
        _localization = new Dictionary<string, Dictionary<string, string>>();
        foreach (var container in containers)
            _localization[container.Locale] = container.KetMessages.ToDictionary();
    }
    
    public string Get(string key, Locale locale)
    {
        if (!_localization.TryGetValue(locale, out Dictionary<string, string>? localeValues))
            throw new InvalidOperationException($"Locale '{locale}' not found");
        if (!localeValues.TryGetValue(key, out var value))
            throw new InvalidOperationException($"Unable to find value for {key} in {locale} locale");
        
        return value;
    }

    public bool TryGet(string key, Locale locale, out string? value)
    {
        value = null;
        if (!_localization.TryGetValue(locale, out Dictionary<string, string>? localeValues))
            return false;
        return localeValues.TryGetValue(key, out value);
    }
}