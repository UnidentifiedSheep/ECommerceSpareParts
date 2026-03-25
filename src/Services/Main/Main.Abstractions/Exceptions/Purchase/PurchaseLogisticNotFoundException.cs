using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Purchase;

public class PurchaseLogisticNotFoundException : NotFoundException, ILocalizableException
{
    public string MessageKey => "purchase.logistics.data.not.found";
    public object[]? Arguments => null;
    public PurchaseLogisticNotFoundException(string purchaseId) 
        : base(null, new { PurchaseId = purchaseId }) { }
}