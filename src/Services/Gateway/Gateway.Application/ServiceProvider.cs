using Abstractions;
using Application.Common;
using Application.Common.Behaviors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZiggyCreatures.Caching.Fusion;

namespace Gateway.Application;

public static class ServiceProvider
{
    public static IServiceCollection AddApplicationLayer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddFusionCache()
            .WithRegisteredDistributedCache()
            .WithRegisteredBackplane()
            .WithSystemTextJsonSerializer();

        services
            .AddApplicationBase(
                ServicesDefinitions.Gateway,
                configuration,
                typeof(ServiceProvider).Assembly,
                typeof(TransactionBehavior<,>),
                typeof(SaveChangesBehavior<,>),
                typeof(IntegrationEventPublisherBehavior<,>),
                typeof(DbValidationBehavior<,>));

        return services;
    }
}
