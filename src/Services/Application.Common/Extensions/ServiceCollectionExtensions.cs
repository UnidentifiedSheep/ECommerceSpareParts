using System.Reflection;
using Abstractions.Interfaces.RelatedData;
using Application.Common.Abstractions;
using Application.Common.Abstractions.RelatedData;
using Application.Common.Interfaces;
using Application.Common.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterRelatedData(this IServiceCollection collection)
    {
        collection.AddScoped<IRelatedDataFactory, RelatedDataFactory>();
        collection.AddScoped<IRelatedDataCollector, RelatedDataCollector>();

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
    
    public static IServiceCollection RegisterDbValidations(this IServiceCollection services, Assembly? assembly = null)
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

    public static IServiceCollection RegisterFluentValidations(
        this IServiceCollection services,
        Assembly? assembly = null)
    {
        assembly ??= Assembly.GetExecutingAssembly();
        services.AddValidatorsFromAssembly(assembly);
        
        return services;
    }
}