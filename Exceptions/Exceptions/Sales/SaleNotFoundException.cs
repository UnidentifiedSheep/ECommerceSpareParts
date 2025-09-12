using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Sales;

public class SaleNotFoundException(string id) : NotFoundException($"Не удалось найти продажу", new { Id = id })
{
    
}