using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Sales;

public class SaleNotFoundException(string saleId) : NotFoundException($"Не удалось найти продажу {saleId}")
{
    
}