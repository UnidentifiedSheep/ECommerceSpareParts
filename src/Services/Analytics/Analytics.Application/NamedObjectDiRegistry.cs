using Analytics.Application.NamedObjects.Analyzers.Markup;
using Analytics.Application.NamedObjects.Metrics;
using Application.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Analytics.Application;

public static class NamedObjectDiRegistry
{
    public static IServiceCollection AddNamedObjects(this IServiceCollection services)
    {
        return services
            .RegisterNamedObject<MetricDefinitionNamedObjectBase>(objectsLifetime: ServiceLifetime.Singleton)
            .RegisterNamedObject<MarkupAnalyzerNamedObjectBase>(objectsLifetime: ServiceLifetime.Scoped);
    }
}