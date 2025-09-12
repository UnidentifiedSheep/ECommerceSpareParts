using Core.Exceptions;
using Exceptions.Base;
using Exceptions.Exceptions;

namespace MonoliteUnicorn.Exceptions.Sales;

public class SalesContentEmptyException() : BadRequestException("Продажа не может не иметь позиций")
{
    
}