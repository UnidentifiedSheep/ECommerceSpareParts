using Localization.Abstractions.Models;

namespace Localization.Abstractions.Interfaces;

public interface IScopedStringLocalizer : IDisposable
{
    Locale Locale { get; }
    string this[string key] { get; }
    void SetLocale(Locale locale);
    string Get(string key);
    bool TryGet(string key, out string? value);
}