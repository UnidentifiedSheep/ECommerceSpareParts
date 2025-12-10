using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Sales;

public class SaleContentNotFoundException : BadRequestException
{
    [ExampleExceptionValues(false, 123)]
    public SaleContentNotFoundException(int id) : base("Не удалось найти позицию в продаже", new { Id = id })
    {
    }
}