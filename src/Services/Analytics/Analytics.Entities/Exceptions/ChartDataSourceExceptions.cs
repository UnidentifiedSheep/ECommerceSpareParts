using Exceptions.Base.Localized;

namespace Analytics.Entities.Exceptions;

public sealed class ChartDataSourceNotFoundException(string systemName) : LocalizedNotFoundException(
	"chart.data.source.not.found",
	new
	{
		SystemName = systemName
	},
	[systemName]);
