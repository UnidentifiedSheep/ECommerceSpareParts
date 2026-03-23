using Localization.Abstractions.Interfaces;
using Localization.Abstractions.Models;

namespace Localization.Domain;

public sealed class ScopedStringLocalizer(IStringLocalizer stringLocalizer) : IScopedStringLocalizer
{
    private Locale? _locale;
    private bool _disposed;
    
    public void SetLocale(Locale locale)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _locale = locale;
    }

    public string Get(string key)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(_locale);
        
        return stringLocalizer.Get(key, _locale.Value);
    }

    public string this[string key] => Get(key);

    public void Dispose()
    {
        _disposed = true;
    }
}