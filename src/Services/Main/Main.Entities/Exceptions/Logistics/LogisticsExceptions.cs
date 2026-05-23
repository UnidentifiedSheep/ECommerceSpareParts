using Exceptions.Base.Localized;

namespace Main.Entities.Exceptions.Logistics;

public class NoLogisticsItemsException()
    : LocalizedBadRequestException("logistics.no.items.for.calculation");
