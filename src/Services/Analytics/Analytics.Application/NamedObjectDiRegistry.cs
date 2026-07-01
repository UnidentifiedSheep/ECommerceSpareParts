using Analytics.Application.NamedObjects;
using Analytics.Application.NamedObjects.Analyzers;
using Analytics.Application.NamedObjects.Analyzers.Markup;
using Analytics.Application.NamedObjects.Metrics;
using Application.Common.Extensions;
using Application.Common.Handlers.NamedObjects;
using Application.Common.Interfaces.NamedObject;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Analytics.Application;

public static class NamedObjectDiRegistry
{
    public static IServiceCollection AddNamedObjects(this IServiceCollection services)
    {
        services.TryAddScoped<
            IRequestHandler<GetNamedObjectsQuery, GetNamedObjectsResult>,
            GetNamedObjectsHandler>();
        
        return services
            .AddSingleton<INamedObjectGroupRegistry, NamedObjectGroupRegistry>()
            .RegisterNamedObject<MetricDefinitionNamedObjectBase>(objectsLifetime: ServiceLifetime.Singleton)
            .RegisterNamedObject<MarkupAnalyzerNamedObjectBase>(objectsLifetime: ServiceLifetime.Scoped);
    }
}