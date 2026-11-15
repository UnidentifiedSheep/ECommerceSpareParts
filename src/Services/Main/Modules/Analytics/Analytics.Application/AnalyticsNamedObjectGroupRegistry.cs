using Main.Application.Analytics.Analyzers;
using Main.Application.Analytics.Charts;
using NamedObject.Core.Base;

namespace Main.Application.Analytics;

public class AnalyticsNamedObjectGroupRegistry : NamedObjectGroupRegistryBase
{
	public AnalyticsNamedObjectGroupRegistry()
	{
		Register<MarkupAnalyzerNamedObjectBase>("MarkupAnalyzer");
		Register<ChartDataSourceNamedObject>("ChartData");
	}
}
