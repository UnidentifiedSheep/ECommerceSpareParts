using Internal.Integration.Core.Models.Main;
using Internal.Integration.Core.Models.Main.Purchase;

namespace Internal.Integration.Core.Interfaces.Main;

public interface IPurchaseNode
{
    Task<InternalFullPurchase?> GetFullPurchase(Guid purchaseId, CancellationToken cancellationToken = default);
}