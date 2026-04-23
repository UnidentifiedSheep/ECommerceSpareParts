using System.Reflection;
using Application.Common.Abstractions;
using Application.Common.Abstractions.Settings;
using Application.Common.Behaviors;
using Application.Common.Extensions;
using Application.Common.Interfaces.Settings;
using Application.Common.Services.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Common;

public static class ServiceProvider
{
    public static IServiceCollection AddApplicationBase(this IServiceCollection services, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetExecutingAssembly();
        services
            .RegisterRelatedData()
            .RegisterIntegrationEventScope()
            .RegisterCachePolicies(assembly)
            .RegisterDbValidations(assembly)
            .RegisterFluentValidations(assembly);

        services.AddSingleton<ISettingsContainer, SettingsContainer>();
        services.AddScoped<ISettingsService, SettingsService>();
        
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