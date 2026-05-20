using Application.Common;
using Application.Common.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using Search.Application.Interfaces;
using Search.Application.Services;

namespace Search.Application;

public static class ServiceProvider
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddApplicationBase(
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

        return services;
    }
}
