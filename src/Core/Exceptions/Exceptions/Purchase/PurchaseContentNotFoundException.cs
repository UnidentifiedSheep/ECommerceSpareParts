using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Purchase;

public class PurchaseContentNotFoundException : NotFoundException
{
    public PurchaseContentNotFoundException(int id) : base("Не удалось найти позицию закупки", new { Id = id })
    {
    }
}