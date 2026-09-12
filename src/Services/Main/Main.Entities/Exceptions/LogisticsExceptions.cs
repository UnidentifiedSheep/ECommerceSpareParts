using Exceptions.Base.Localized;

namespace Main.Entities.Exceptions;

public class NoLogisticsItemsException() : LocalizedBadRequestException(LogisticsNoItemsForCalculationMessage.Instance);
