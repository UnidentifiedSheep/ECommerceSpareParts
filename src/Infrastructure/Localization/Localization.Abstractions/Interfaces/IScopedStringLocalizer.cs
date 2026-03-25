using Localization.Abstractions.Models;

namespace Localization.Abstractions.Interfaces;

public interface IScopedStringLocalizer : IDisposable
{
    void SetLocale(Locale locale);
    string Get(string key);
    bool TryGet(string key, out string? value);
    string this[string key] { get; }
}