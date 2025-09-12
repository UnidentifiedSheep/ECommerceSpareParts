using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Sales;

public class SaleContentNotFoundException(int id) : BadRequestException($"Не удалось найти позицию в продаже", new { Id = id })
{
    
}