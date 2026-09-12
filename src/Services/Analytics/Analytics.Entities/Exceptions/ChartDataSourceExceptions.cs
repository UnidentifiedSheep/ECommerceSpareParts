using Exceptions.Base.Localized;

namespace Analytics.Entities.Exceptions;

public sealed class ChartDataSourceNotFoundException(string systemName)
	: LocalizedNotFoundException(
		ChartDataSourceNotFoundMessage.Create(systemName),
		new { SystemName = systemName });
