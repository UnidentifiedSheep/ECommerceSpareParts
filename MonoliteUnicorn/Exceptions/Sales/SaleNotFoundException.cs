using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Sales;

public class SaleNotFoundException(string id) : NotFoundException($"Не удалось найти продажу", new { Id = id })
{
    
}