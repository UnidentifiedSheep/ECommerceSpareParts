using Abstractions.Interfaces.Localization;

namespace Localization.Domain;

public sealed class ScopedStringLocalizer(IStringLocalizer stringLocalizer) : IScopedStringLocalizer
{
    private string? _locale;
    private bool _disposed;
    
    public void SetLocale(string locale)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _locale = locale;
    }

    public string Get(string key)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(_locale);
        
        return stringLocalizer.Get(key, _locale);
    }

    public string this[string key] => Get(key);

    public void Dispose()
    {
        _disposed = true;
    }
}