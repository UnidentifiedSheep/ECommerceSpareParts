using Exceptions.Base.Localized;

namespace Main.Entities.Exceptions.Analytics;

public sealed class ChartDataSourceNotFoundException(string systemName) : LocalizedNotFoundException(
	ChartDataSourceNotFoundMessage.Create(systemName),
	new
	{
		SystemName = systemName
	});
