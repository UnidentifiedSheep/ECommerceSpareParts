using Exceptions.Base;

namespace Exceptions.Exceptions.Sales;

public class SaleNotFoundException(string id) : NotFoundException("Не удалось найти продажу", new { Id = id })
{
}