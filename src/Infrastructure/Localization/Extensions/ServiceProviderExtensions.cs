using Abstractions.Interfaces.Localization;
using Localization.Middlewares;
using Microsoft.Extensions.DependencyInjection;

namespace Localization.Extensions;

public static class ServiceProviderExtensions
{
    public static IServiceCollection AddLocalization(this IServiceCollection services, params string[] locales)
    {
        services.AddLocales(locales)
            .AddStringLocalizer()
            .AddScopedStringLocalizer();

        services.AddScoped<ScopedLocalizationMiddleware>();
        
        return services;
    }
    
    public static IServiceCollection AddLocales(this IServiceCollection services, params string[] locales)
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