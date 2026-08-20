using Analytics.Application.NamedObjects.Analyzers;
using Application.Common.Abstractions.NamedObjects;

namespace Analytics.Application.NamedObjects;

public class NamedObjectGroupRegistry : NamedObjectGroupRegistryBase
{
    public NamedObjectGroupRegistry()
    {
        Register<MarkupAnalyzerNamedObjectBase>("MarkupAnalyzer");
    }
}
