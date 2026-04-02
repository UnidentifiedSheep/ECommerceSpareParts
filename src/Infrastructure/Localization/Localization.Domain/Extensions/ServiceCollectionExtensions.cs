using Localization.Abstractions.Interfaces;
using Localization.Abstractions.Models;
using Localization.Domain.Middlewares;
using Microsoft.Extensions.DependencyInjection;

namespace Localization.Domain.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLocalization(
        this IServiceCollection services, 
        Locale defaultLocale,
        params Locale[] locales)
    {
        services.AddLocales(locales)
            .AddStringLocalizer()
            .AddScopedStringLocalizer();

        services.AddScoped<ScopedLocalizationMiddleware>(sp => 
            new ScopedLocalizationMiddleware(sp.GetRequiredService<IScopedStringLocalizer>()));

        ScopedLocalizationMiddleware.Configure(defaultLocale, locales);
        
        return services;
    }

    public static IServiceCollection AddLocales(this IServiceCollection services, params Locale[] locales)
    {
        foreach (var locale in locales)
            services.AddSingleton<ILocalizerContainer, LocalizerContainer>(_ => new LocalizerContainer(locale));
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
}