using System.Reflection;
using Application.Common.Backplane;
using Application.Common.Behaviors;
using Application.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;
using ZiggyCreatures.Caching.Fusion.Backplane;

namespace Application.Common;

public static class ServiceProvider
{
    public static IServiceCollection AddApplicationBase(this IServiceCollection services, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetExecutingAssembly();
        services
            .RegisterIdCollector()
            .RegisterIntegrationEventScope()
            .RegisterCachePolicies(assembly)
            .RegisterDbValidations(assembly)
            .RegisterFluentValidations(assembly);

        services.AddSingleton<IBackplaneDispatcher, BackplaneDispatcher>();
        services.AddScoped<IFusionCacheBackplane, MassTransitBackplane>();

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(DbValidationBehavior<,>), ServiceLifetime.Scoped);
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(CacheBehavior<,>));
            config.AddOpenBehavior(typeof(TransactionBehavior<,>), ServiceLifetime.Scoped);
            config.AddOpenBehavior(typeof(IntegrationEventPublisherBehavior<,>), ServiceLifetime.Scoped);
            config.AddOpenBehavior(typeof(SaveChangesBehavior<,>), ServiceLifetime.Scoped);
        });

        return services;
    }
}
