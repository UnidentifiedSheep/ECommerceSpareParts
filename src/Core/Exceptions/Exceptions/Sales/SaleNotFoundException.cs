using Exceptions.Base;

namespace Exceptions.Exceptions.Sales;

public class SaleNotFoundException : NotFoundException
{
    public SaleNotFoundException(string id) : base("Не удалось найти продажу", new { Id = id })
    {
    }
}