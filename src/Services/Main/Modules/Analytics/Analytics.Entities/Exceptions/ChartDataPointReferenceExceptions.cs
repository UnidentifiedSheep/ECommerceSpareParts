using Exceptions.Base.Localized;

namespace Main.Entities.Analytics.Exceptions;

public sealed class ChartDataPointReferenceInvalidException(string field, string entityName)
	: LocalizedBadRequestException(
		ChartDataPointReferenceInvalidMessage.Create(field, entityName),
		new
		{
			Field = field,
			EntityName = entityName
		});
