using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions;

public class SaleContentIsEmptyException() : BadRequestException("В закупке позиции не могут отсутствовать")
{
    
}