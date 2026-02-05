using Exceptions.Base;

namespace Exceptions.Exceptions.Purchase;

public class PurchaseLogisticNotFoundException : NotFoundException
{
    public PurchaseLogisticNotFoundException(string purchaseId) 
        : base("Не удалось найти логистику для продажи.", new { PurchaseId = purchaseId }) { }
}