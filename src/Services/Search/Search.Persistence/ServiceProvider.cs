using Microsoft.Extensions.DependencyInjection;

namespace Search.Persistence;

public static class ServiceProvider
{
    public static IServiceCollection AddPersistenceLayer(this IServiceCollection services)
    {


        return services;
    }
}