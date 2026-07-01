using Localization.Abstractions.Interfaces;
using Localization.Abstractions.Models;

namespace Localization.Domain;

public sealed class ScopedStringLocalizer(IStringLocalizer stringLocalizer) : IScopedStringLocalizer
{
    private bool _disposed;
    private Locale? _locale;

    public Locale Locale => _locale ?? throw new ArgumentNullException(nameof(Locale));

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

    public string Get(string key, params object[] arguments)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(_locale);

        return stringLocalizer.Get(
            key,
            _locale.Value,
            arguments);
    }

    public bool TryGet(string key, out string? value)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(_locale);

        return stringLocalizer.TryGet(
            key,
            _locale.Value,
            out value);
    }

    public bool TryGet(
        string key,
        out string? value,
        params object[] arguments)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(_locale);

        return stringLocalizer.TryGet(
            key,
            _locale.Value,
            out value,
            arguments);
    }

    public string? GetOrDefault(string key) { return TryGet(key, out var value) ? value : null; }

    public string? GetOrDefault(string key, params object[] arguments)
    {
        return TryGet(
            key,
            out var value,
            arguments)
            ? value
            : null;
    }

    public string this[string key] => Get(key);

    public void Dispose() { _disposed = true; }
}