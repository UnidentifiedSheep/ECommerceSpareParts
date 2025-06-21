using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Sales;

public class SalesContentEmptyException() : BadRequestException("Продажа не может не иметь позиций")
{
    
}