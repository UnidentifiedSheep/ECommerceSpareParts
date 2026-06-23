using Internal.Integration.Core.Models.Main;

namespace Internal.Integration.Core.Interfaces.Main;

public interface IPurchaseNode
{
    Task<InternalFullPurchase?> GetFullPurchase(Guid purchaseId, CancellationToken cancellationToken = default);
}