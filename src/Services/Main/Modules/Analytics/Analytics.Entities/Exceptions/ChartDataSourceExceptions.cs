using Exceptions.Base.Localized;

namespace Main.Entities.Analytics.Exceptions;

public sealed class ChartDataSourceNotFoundException(string systemName) : LocalizedNotFoundException(
	ChartDataSourceNotFoundMessage.Create(systemName),
	new
	{
		SystemName = systemName
	});
