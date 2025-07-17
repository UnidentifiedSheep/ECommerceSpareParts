using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Sales;

public class SaleContentNotFoundException(int id) : BadRequestException($"Не удалось найти позицию в продаже", new { Id = id })
{
    
}