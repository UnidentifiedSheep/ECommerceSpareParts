using System.Reflection;
using Application.Common.Backplane;
using Application.Common.Behaviors;
using Application.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;
using ZiggyCreatures.Caching.Fusion.Backplane;

namespace Application.Common;

public static class ServiceProvider
{
    public static IServiceCollection AddApplicationBase(
        this IServiceCollection services, 
        Assembly? assembly = null,
        params Type[] behaviorsToExclude)
    {
        assembly ??= Assembly.GetExecutingAssembly();
        services
            .RegisterIdCollector()
            .RegisterIntegrationEventScope()
            .RegisterCachePolicies(assembly)
            .RegisterDbValidations(assembly)
            .RegisterFluentValidations(assembly);

        services.AddSingleton<IBackplaneDispatcher, BackplaneDispatcher>();
        services.AddSingleton<IFusionCacheBackplane, MassTransitBackplane>();
        
        var hs = behaviorsToExclude.ToHashSet();
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
            config
                .RegisterIfNotExcluded(
                    hs,
                    typeof(ValidationBehavior<,>))
                .RegisterIfNotExcluded(
                    hs,
                    typeof(DbValidationBehavior<,>),
                    ServiceLifetime.Scoped)
                .RegisterIfNotExcluded(
                    hs,
                    typeof(LoggingBehavior<,>))
                .RegisterIfNotExcluded(
                    hs,
                    typeof(CacheBehavior<,>))
                .RegisterIfNotExcluded(
                    hs,
                    typeof(TransactionBehavior<,>),
                    ServiceLifetime.Scoped)
                .RegisterIfNotExcluded(
                    hs,
                    typeof(SaveChangesBehavior<,>),
                    ServiceLifetime.Scoped)
                .RegisterIfNotExcluded(
                    hs,
                    typeof(IntegrationEventPublisherBehavior<,>),
                    ServiceLifetime.Scoped);
        });

        return services;
    }

    private static MediatRServiceConfiguration RegisterIfNotExcluded(
        this MediatRServiceConfiguration serviceConfiguration,
        HashSet<Type> excludedTypes,
        Type openBehaviorType, 
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        if (excludedTypes.Contains(openBehaviorType)) return serviceConfiguration;
        serviceConfiguration.AddOpenBehavior(openBehaviorType, serviceLifetime);
        return serviceConfiguration;
    }
}
