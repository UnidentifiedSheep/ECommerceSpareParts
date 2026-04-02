using System.Collections.Concurrent;
using Localization.Abstractions.Interfaces;
using Localization.Abstractions.Models;
using Microsoft.AspNetCore.Http;

namespace Localization.Domain.Middlewares;

public class ScopedLocalizationMiddleware(IScopedStringLocalizer localizer) : IMiddleware
{
    private static readonly ConcurrentDictionary<string, Locale> Cache = new();

    private static Locale _defaultLocale;
    private static HashSet<Locale> _locales = [];

    private static int _cacheSize;
    private const int MaxCacheSize = 2000;

    private static readonly Lock LockObj = new();
    private static bool _initialized;

    public static void Configure(Locale defaultLocale, Locale[] locales)
    {
        lock (LockObj)
        {
            if (_initialized)
                throw new InvalidOperationException("Middleware is already initialized");

            if (locales == null || locales.Length == 0)
                throw new ArgumentException("Locales must not be empty");

            if (locales.Any(l => l == default))
                throw new ArgumentException("Locales contain invalid values");

            _defaultLocale = defaultLocale;
            _locales = new HashSet<Locale>(locales);
            _initialized = true;
        }
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        EnsureInitialized();

        var locale = _defaultLocale;

        if (context.Request.Headers.TryGetValue("Accept-Language", out var header))
            locale = ResolveLocale(header.ToString());
        
        localizer.SetLocale(locale);

        await next(context);
    }

    private static void EnsureInitialized()
    {
        if (!_initialized)
            throw new InvalidOperationException("Middleware is not configured. Call Configure() during startup.");
    }

    private Locale ResolveLocale(string header)
    {
        if (Cache.TryGetValue(header, out var cached))
            return cached;

        var languages = header
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var lang in languages)
        {
            var candidate = new Locale(lang.Split(';')[0].Trim());

            if (_locales.Contains(candidate))
            {
                AddToCache(header, candidate);
                return candidate;
            }
        }

        AddToCache(header, _defaultLocale);
        return _defaultLocale;
    }

    private static void AddToCache(string key, Locale value)
    {
        if (Interlocked.Increment(ref _cacheSize) > MaxCacheSize)
        {
            lock (LockObj)
            {
                if (_cacheSize > MaxCacheSize)
                {
                    Cache.Clear();
                    _cacheSize = 0;
                }
            }
        }

        Cache[key] = value;
    }
}