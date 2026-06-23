using Application.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Pricing.Application;

public static class ServiceProvider
{
    public static IServiceCollection AddApplicationLayer(
        this IServiceCollection collection,
        IConfiguration configuration)
    {
        collection.AddApplicationBase(configuration, typeof(Global).Assembly);

        

        return collection;
    }
}