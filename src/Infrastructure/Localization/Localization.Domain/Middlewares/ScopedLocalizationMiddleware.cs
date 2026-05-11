using System.Collections.Concurrent;
using Localization.Abstractions.Interfaces;
using Localization.Abstractions.Models;
using Microsoft.AspNetCore.Http;

namespace Localization.Domain.Middlewares;

public class ScopedLocalizationMiddleware(
    Locale defaultLocale,
    HashSet<Locale> locales,
    IScopedStringLocalizer localizer) : IMiddleware
{
    private const int MaxCacheSize = 2000;
    private static readonly ConcurrentDictionary<string, Locale> Cache = new();
    private static readonly Lock LockObj = new();

    private static int _cacheSize;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var locale = defaultLocale;

        if (context.Request.Headers.TryGetValue("Accept-Language", out var header))
            locale = ResolveLocale(header.ToString());

        localizer.SetLocale(locale);

        await next(context);
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

            if (locales.Contains(candidate))
            {
                AddToCache(header, candidate);
                return candidate;
            }
        }

        AddToCache(header, defaultLocale);
        return defaultLocale;
    }

    private static void AddToCache(string key, Locale value)
    {
        if (Interlocked.Increment(ref _cacheSize) > MaxCacheSize)
            lock (LockObj)
            {
                if (_cacheSize > MaxCacheSize)
                {
                    Cache.Clear();
                    _cacheSize = 0;
                }
            }

        Cache[key] = value;
    }
}