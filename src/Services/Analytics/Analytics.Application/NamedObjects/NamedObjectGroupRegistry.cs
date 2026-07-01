using Analytics.Application.NamedObjects.Analyzers;
using Analytics.Application.NamedObjects.Analyzers.Markup;
using Analytics.Application.NamedObjects.Metrics;
using Application.Common.Abstractions.NamedObjects;

namespace Analytics.Application.NamedObjects;

public class NamedObjectGroupRegistry : NamedObjectGroupRegistryBase
{
    public NamedObjectGroupRegistry()
    {
        Register<MetricDefinitionNamedObjectBase>("MetricDefinition");
        Register<MarkupAnalyzerNamedObjectBase>("MarkupAnalyzer");
    }
}