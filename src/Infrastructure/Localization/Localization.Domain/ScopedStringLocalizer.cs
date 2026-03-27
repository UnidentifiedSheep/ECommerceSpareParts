using Localization.Abstractions.Interfaces;
using Localization.Abstractions.Models;

namespace Localization.Domain;

public sealed class ScopedStringLocalizer(IStringLocalizer stringLocalizer) : IScopedStringLocalizer
{
    private bool _disposed;
    private Locale? _locale;

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

    public bool TryGet(string key, out string? value)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(_locale);

        return stringLocalizer.TryGet(key, _locale.Value, out value);
    }

    public string this[string key] => Get(key);

    public void Dispose()
    {
        _disposed = true;
    }
}