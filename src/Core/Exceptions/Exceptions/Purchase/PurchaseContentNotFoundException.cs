using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Purchase;

public class PurchaseContentNotFoundException : NotFoundException
{
    [ExampleExceptionValues(false, 123)]
    public PurchaseContentNotFoundException(int id) : base("Не удалось найти позицию закупки", new { Id = id })
    {
    }
}