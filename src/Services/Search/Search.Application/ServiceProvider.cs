using Application.Common;
using Application.Common.Behaviors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Search.Application.Configs;
using Search.Application.Interfaces;
using Search.Application.Interfaces.Producer;
using Search.Application.Interfaces.Product;
using Search.Application.Services;
using Search.Application.Services.IndexSynchronizers;
using Search.Entities;

namespace Search.Application;

public static class ServiceProvider
{
    public static IServiceCollection AddApplicationLayer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        SortByConfig.Configure();
        
        services
            .AddApplicationBase(
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

        services.AddSingleton<IIndexSynchronizer<Product, int>, ProductIndexSynchronizer>();
        services.AddSingleton<IIndexSynchronizer<Producer, int>, ProducerIndexSynchronizer>();

        return services;
    }
}
