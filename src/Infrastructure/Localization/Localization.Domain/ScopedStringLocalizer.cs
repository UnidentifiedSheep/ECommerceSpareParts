using Localization.Abstractions.Interfaces;
using Localization.Abstractions.Models;
using Microsoft.Extensions.Options;

namespace Localization.Domain;

public sealed class ScopedStringLocalizer(
    IStringLocalizer stringLocalizer,
    IOptions<LocalesOptions>? localeOptions = null) : IScopedStringLocalizer
{
    private bool _disposed;
    private Locale? _locale;
    private readonly Locale? _defaultLocale = GetDefaultLocale(localeOptions);

    public Locale Locale => _locale
                            ?? _defaultLocale
                            ?? throw new ArgumentNullException(nameof(Locale));

    public void SetLocale(Locale locale)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _locale = locale;
    }

    public string Get(string key)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return stringLocalizer.Get(key, Locale);
    }

    public string Get(string key, params object[] arguments)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return stringLocalizer.Get(
            key,
            Locale,
            arguments);
    }

    public bool TryGet(string key, out string? value)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return stringLocalizer.TryGet(
            key,
            Locale,
            out value);
    }

    public bool TryGet(
        string key,
        out string? value,
        params object[] arguments)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return stringLocalizer.TryGet(
            key,
            Locale,
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

    private static Locale? GetDefaultLocale(
        IOptions<LocalesOptions>? localeOptions)
    {
        var defaultLocale = localeOptions?.Value.Default;
        return string.IsNullOrWhiteSpace(defaultLocale)
            ? (Locale?)null
            : new Locale(defaultLocale);
    }
}
