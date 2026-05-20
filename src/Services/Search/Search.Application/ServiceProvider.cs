using Microsoft.Extensions.DependencyInjection;
using Search.Application.Interfaces;
using Search.Application.Services;

namespace Search.Application;

public static class ServiceProvider
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddSingleton<IProductIndexSynchronizer, ProductIndexSynchronizer>();

        return services;
    }
}
