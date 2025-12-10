using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Purchase;

public class PurchaseNotFoundException : NotFoundException
{
    [ExampleExceptionValues(false, "0000-0000-0000-0000")]
    public PurchaseNotFoundException(string id) : base("Не удалось найти закупку", new { Id = id })
    {
    }
}