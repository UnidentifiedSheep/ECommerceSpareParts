using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Purchase;

public class PurchaseContentEmptyException() : BadRequestException("Закупка не может не иметь позиций")
{
    
}