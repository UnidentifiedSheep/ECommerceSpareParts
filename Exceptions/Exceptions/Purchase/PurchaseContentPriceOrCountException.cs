using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Purchase;

public class PurchaseContentPriceOrCountException() : BadRequestException("Цена или количество у позиции в закупке не может быть равно 0")
{
    
}