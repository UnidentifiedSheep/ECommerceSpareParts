using Exceptions.Base;

namespace Exceptions.Exceptions.Sales;

public class SaleContentNotFoundException(int id) : BadRequestException($"Не удалось найти позицию в продаже", new { Id = id })
{
    
}