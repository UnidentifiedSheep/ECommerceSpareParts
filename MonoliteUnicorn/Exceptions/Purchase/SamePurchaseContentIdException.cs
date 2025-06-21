using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Purchase;

public class SamePurchaseContentIdException() : BadRequestException($"Одинаковый айди позиции не допустим для редактирования закупок")
{
    
}