using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Purchase;

public class PurchaseContentEmptyException() : BadRequestException("Закупка не может не иметь позиций")
{
    
}