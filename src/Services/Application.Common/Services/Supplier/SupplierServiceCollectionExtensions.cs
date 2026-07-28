using Application.Common.Services.Supplier.Favorite;
using Application.Common.Services.Supplier.Tmtr;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Common.Services.Supplier;

public static class SupplierServiceCollectionExtensions
{
    public static IServiceCollection AddMainSupplierSettingProviders(
        this IServiceCollection services)
    {
        services.AddScoped<FavoriteMainSettingProvider>();
        services.AddScoped<TmtrMainSettingProvider>();

        return services;
    }
}
