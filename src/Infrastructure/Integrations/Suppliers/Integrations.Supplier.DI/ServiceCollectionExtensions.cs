using Integrations.Common;
using Integrations.Favorit;
using Integrations.Supplier.Connections;
using Integrations.Supplier.Interfaces;
using Integrations.Supplier.Settings;
using Integrations.Tmtr;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Integrations.Supplier.DI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFavoriteIntegration<TConnectionProvider, TSettingsProvider>(
        this IServiceCollection services)
        where TConnectionProvider : class, IConnectionProvider<FavoritConnection>
        where TSettingsProvider : class, ISupplierSettingsProvider<FavoriteSettings>
    {
        services.AddSupplierBase();

        services.AddScoped<IConnectionProvider<FavoritConnection>, TConnectionProvider>();
        services.AddScoped<ISupplierSettingsProvider<FavoriteSettings>, TSettingsProvider>();

        services.AddHttpClient<IFavoritPartsClient, FavoritPartsClient>()
            .AddDefaultResilenceHandler();

        services.AddScoped<ISupplier, FavoritPartsSupplier>();

        return services;
    }
    
    public static IServiceCollection AddTmtrIntegration<TConnectionProvider, TSettingsProvider>(
        this IServiceCollection services)
        where TConnectionProvider : class, IConnectionProvider<TmtrConnection>
        where TSettingsProvider : class, ISupplierSettingsProvider<TmtrSettings>
    {
        services.AddSupplierBase();

        services.AddScoped<IConnectionProvider<TmtrConnection>, TConnectionProvider>();
        services.AddScoped<ISupplierSettingsProvider<TmtrSettings>, TSettingsProvider>();

        services.AddHttpClient<ITmtrClient, TmtrClient>()
            .AddDefaultResilenceHandler();

        services.AddScoped<ISupplier, TmtrSupplier>();

        return services;
    }

    public static IServiceCollection AddSupplierBase(this IServiceCollection services)
    {
        services.TryAddScoped<ISupplierFactory, SupplierFactory>();
        return services;
    }
}