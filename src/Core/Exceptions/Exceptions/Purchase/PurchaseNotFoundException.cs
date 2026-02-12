using Exceptions.Base;

namespace Exceptions.Exceptions.Purchase;

public class PurchaseNotFoundException : NotFoundException
{
    public PurchaseNotFoundException(string id) : base("Не удалось найти закупку", new { Id = id })
    {
    }
}