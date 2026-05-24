using Application.Common;
using Application.Common.Behaviors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Search.Application.Interfaces;
using Search.Application.Interfaces.Producer;
using Search.Application.Interfaces.Product;
using Search.Application.Services;

namespace Search.Application;

public static class ServiceProvider
{
    public static IServiceCollection AddApplicationLayer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddApplicationBase(
            configuration: configuration,
            assembly: typeof(ServiceProvider).Assembly,
            behaviorsToExclude:
            [
                typeof(TransactionBehavior<,>),
                typeof(SaveChangesBehavior<,>),
                typeof(IntegrationEventPublisherBehavior<,>),
                typeof(DbValidationBehavior<,>),
                typeof(CacheBehavior<,>)
            ]);

        services.AddSingleton<IProductIndexSynchronizer, ProductIndexSynchronizer>();
        services.AddSingleton<IProducerIndexSynchronizer, ProducerIndexSynchronizer>();

        return services;
    }
}
