using Exceptions.Base;

namespace Exceptions.Exceptions.Logistics;

public class NoLogisticsItemsException : BadRequestException
{
    public NoLogisticsItemsException() : base("Ни одна позиция не подходит для расчёта логистики") { }
}