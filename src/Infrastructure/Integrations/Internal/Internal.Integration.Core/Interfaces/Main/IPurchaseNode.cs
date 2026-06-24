using Internal.Integration.Core.Models;
using Internal.Integration.Core.Models.Main;
using Internal.Integration.Core.Models.Main.Purchase;

namespace Internal.Integration.Core.Interfaces.Main;

public interface IPurchaseNode
{
    Task<InternalResponse<InternalFullPurchase>> GetFullPurchase(Guid purchaseId, CancellationToken cancellationToken = default);
}