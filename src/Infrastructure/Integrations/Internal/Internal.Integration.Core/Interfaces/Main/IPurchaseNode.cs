using Integrations.Common;
using Internal.Integration.Core.Models.Main.Purchase;

namespace Internal.Integration.Core.Interfaces.Main;

public interface IPurchaseNode
{
    Task<Response<InternalFullPurchase>> GetFullPurchase(
        Guid purchaseId,
        CancellationToken cancellationToken = default);
}