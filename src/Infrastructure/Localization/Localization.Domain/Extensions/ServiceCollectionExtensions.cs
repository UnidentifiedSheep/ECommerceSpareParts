using Localization.Abstractions.Interfaces;
using Localization.Abstractions.Models;
using Localization.Domain.Middlewares;
using Localization.Domain.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Localization.Domain.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLocalization(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<LocalesOptions>()
            .Bind(configuration.GetRequiredSection(LocalesOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(
                x => x.Supported.Length > 0,
                $"{nameof(LocalesOptions.Supported)} must contain at least one locale")
            .Validate(
                x => x.Supported.Distinct(StringComparer.OrdinalIgnoreCase).Count() == x.Supported.Length,
                $"{nameof(LocalesOptions.Supported)} must not contain duplicates")
            .Validate(
                x => x.Supported.Contains(x.Default, StringComparer.OrdinalIgnoreCase),
                $"{nameof(LocalesOptions.Default)} must be included in {nameof(LocalesOptions.Supported)}")
            .ValidateOnStart();

        var options = configuration
                          .GetRequiredSection(LocalesOptions.SectionName)
                          .Get<LocalesOptions>()
                      ?? throw new InvalidOperationException(
                          $"Missing {LocalesOptions.SectionName} configuration section");

        return services.AddLocalization(
            options.Default,
            options.Supported.Select(x => (Locale)x).ToArray());
    }

    public static IServiceCollection AddLocalization(
        this IServiceCollection services,
        Locale defaultLocale,
        params Locale[] locales)
    {
        services.AddLocales(locales)
            .AddStringLocalizer()
            .AddScopedStringLocalizer()
            .AddLocalizableJsonSerializer();

        var hs = locales.ToHashSet();

        services.AddScoped<ScopedLocalizationMiddleware>(sp =>
            new ScopedLocalizationMiddleware(
                defaultLocale,
                hs,
                sp.GetRequiredService<IScopedStringLocalizer>()));

        return services;
    }

    public static IServiceCollection AddLocales(this IServiceCollection services, params Locale[] locales)
    {
        foreach (var locale in locales)
            services.AddSingleton<ILocalizerContainer, LocalizerContainer>(_ =>
                new LocalizerContainer(locale));
        return services;
    }

    public static IServiceCollection AddStringLocalizer<TLocalizer>(this IServiceCollection services)
        where TLocalizer : class, IStringLocalizer
    {
        services.AddSingleton<IStringLocalizer, TLocalizer>();
        return services;
    }

    public static IServiceCollection AddStringLocalizer(this IServiceCollection services)
    {
        return services.AddStringLocalizer<StringLocalizer>();
    }

    public static IServiceCollection AddScopedStringLocalizer<TLocalizer>(this IServiceCollection services)
        where TLocalizer : class, IScopedStringLocalizer
    {
        services.AddScoped<IScopedStringLocalizer, TLocalizer>();
        return services;
    }

    public static IServiceCollection AddScopedStringLocalizer(this IServiceCollection services)
    {
        return services.AddScopedStringLocalizer<ScopedStringLocalizer>();
    }

    public static IServiceCollection AddLocalizableJsonSerializer(this IServiceCollection services)
    {
        return services.AddScoped<IScopedLocalizedJsonSerializer, ScopedLocalizedJsonSerializer>();
    }
}