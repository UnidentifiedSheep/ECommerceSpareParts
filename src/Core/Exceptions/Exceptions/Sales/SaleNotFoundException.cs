using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Sales;

public class SaleNotFoundException : NotFoundException
{
    [ExampleExceptionValues(false, "0000-0000-0000-0000")]
    public SaleNotFoundException(string id) : base("Не удалось найти продажу", new { Id = id })
    {
    }
}