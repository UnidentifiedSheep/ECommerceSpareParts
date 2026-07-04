using System.Reflection;
using Application.Common.Abstractions;
using Application.Common.Handlers.Settings;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.NamedObject;
using Application.Common.Interfaces.Settings;
using Application.Common.NamedObject;
using Application.Common.Services;
using Application.Common.Services.Events;
using Application.Common.Services.Settings;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Application.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterIdCollector(this IServiceCollection collection)
    {
        collection.AddScoped<IIdsCollector, IdsCollector>();
        return collection;
    }

    public static IServiceCollection RegisterSettingsService<TSettingFactory>(
        this IServiceCollection collection)
        where TSettingFactory : class, ISettingFactory
    {
        collection.AddSingleton<ISettingsContainer, SettingsContainer>();
        collection.AddScoped<ISettingsService, SettingsService>();
        collection.AddSingleton<ISettingFactory, TSettingFactory>();

        collection.TryAddScoped<
            IRequestHandler<UpdateSettingCommand, UpdateSettingResult>,
            UpdateSettingHandler>();

        collection.TryAddScoped<
            IRequestHandler<GetSettingsQuery, GetSettingsResult>,
            GetSettingsHandler>();

        return collection;
    }

    public static IServiceCollection RegisterCachePolicies(
        this IServiceCollection services,
        Assembly? assembly = null)
    {
        assembly ??= Assembly.GetExecutingAssembly();

        var types = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .Select(t => new
            {
                Implementation = t,
                Interfaces = t.GetInterfaces()
                    .Where(i => i.IsGenericType &&
                                i.GetGenericTypeDefinition() == typeof(ICachePolicy<>))
            })
            .Where(x => x.Interfaces.Any());

        foreach (var type in types)
        foreach (var @interface in type.Interfaces)
            services.AddScoped(@interface, type.Implementation);

        return services;
    }

    public static IServiceCollection RegisterDbValidations(
        this IServiceCollection services,
        Assembly? assembly = null)
    {
        assembly ??= Assembly.GetExecutingAssembly();
        var validationTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .Where(t => t.BaseType != null
                        && t.BaseType.IsGenericType
                        && t.BaseType.GetGenericTypeDefinition() == typeof(AbstractDbValidation<>));

        foreach (var type in validationTypes)
        {
            var baseType = type.BaseType!;
            services.AddScoped(baseType, type);
        }

        return services;
    }

    public static IServiceCollection RegisterIntegrationEventScope(this IServiceCollection services)
    {
        services.AddScoped<IIntegrationEventScope, IntegrationEventScope>();
        return services;
    }

    public static IServiceCollection RegisterDomainEventScope(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventScope, DomainEventScope>();
        return services;
    }

    public static IServiceCollection RegisterFluentValidations(
        this IServiceCollection services,
        Assembly? assembly = null)
    {
        assembly ??= Assembly.GetExecutingAssembly();
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }

    public static IServiceCollection RegisterNamedObject<TBaseObject>(
        this IServiceCollection services,
        Assembly? assembly = null,
        ServiceLifetime objectsLifetime = ServiceLifetime.Scoped)
        where TBaseObject : class, INamedObject
    {
        assembly ??= typeof(TBaseObject).Assembly;

        services.Scan(scan =>
        {
            var registration = scan
                .FromAssemblies(assembly)
                .AddClasses(classes => classes.AssignableTo<TBaseObject>())
                .As<TBaseObject>();

            switch (objectsLifetime)
            {
                case ServiceLifetime.Singleton:
                    registration.WithSingletonLifetime();
                    break;

                case ServiceLifetime.Scoped:
                    registration.WithScopedLifetime();
                    break;

                case ServiceLifetime.Transient:
                    registration.WithTransientLifetime();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(objectsLifetime),
                        objectsLifetime,
                        null);
            }
        });

        services.TryAddScoped(typeof(INamedObjectRegistry<>), typeof(NamedObjectRegistry<>));
        services.TryAddScoped<INamedObjectGroupResolver, NamedObjectGroupResolver>();

        return services;
    }
}