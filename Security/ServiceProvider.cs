using Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Security;

public static class ServiceProvider
{
    public static IServiceCollection AddSecurityLayer(this IServiceCollection collection)
    {
        collection.AddSingleton<IJwtGenerator, JwtGenerator>();
        return collection;
    }
}