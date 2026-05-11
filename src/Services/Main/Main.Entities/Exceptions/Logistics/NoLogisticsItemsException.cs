using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Entities.Exceptions.Logistics;

public class NoLogisticsItemsException() : BadRequestException(null), ILocalizableException
{
    public string MessageKey => "logistics.no.items.for.calculation";
    public object[]? Arguments => null;
}