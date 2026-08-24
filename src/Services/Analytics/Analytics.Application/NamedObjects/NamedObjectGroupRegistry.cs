using Analytics.Application.NamedObjects.Analyzers;
using Analytics.Application.NamedObjects.ChartDataSlices;
using Application.Common.Abstractions.NamedObjects;

namespace Analytics.Application.NamedObjects;

public class NamedObjectGroupRegistry : NamedObjectGroupRegistryBase
{
    public NamedObjectGroupRegistry()
    {
        Register<MarkupAnalyzerNamedObjectBase>("MarkupAnalyzer");
        Register<ChartDataSourceNamedObject>("ChartData");
    }
}
