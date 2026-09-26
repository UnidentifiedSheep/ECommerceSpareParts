using Analytics.Application.NamedObjects.Analyzers;
using Analytics.Application.NamedObjects.ChartDataSources;
using NamedObject.Core.Base;

namespace Analytics.Application.NamedObjects;

public class NamedObjectGroupRegistry : NamedObjectGroupRegistryBase
{
	public NamedObjectGroupRegistry()
	{
		Register<MarkupAnalyzerNamedObjectBase>("MarkupAnalyzer");
		Register<ChartDataSourceNamedObject>("ChartData");
	}
}
