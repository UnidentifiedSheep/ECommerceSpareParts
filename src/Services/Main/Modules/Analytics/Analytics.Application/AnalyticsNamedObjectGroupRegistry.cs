using Main.Modules.Analytics.Abstractions.Analyses;
using Main.Modules.Analytics.Abstractions.Analyzers;
using Main.Modules.Analytics.Abstractions.Charts;
using NamedObject.Core.Base;

namespace Main.Modules.Analytics.Application;

public class AnalyticsNamedObjectGroupRegistry : NamedObjectGroupRegistryBase
{
	public AnalyticsNamedObjectGroupRegistry()
	{
		Register<MarkupAnalyzerNamedObjectBase>("MarkupAnalyzer");
		Register<ChartDataSourceNamedObject>("ChartData");
		Register<AnalysisNamedObject>("Analysis");
	}
}
