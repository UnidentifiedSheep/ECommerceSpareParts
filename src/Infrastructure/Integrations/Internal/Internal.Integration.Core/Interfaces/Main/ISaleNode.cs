using Internal.Integration.Core.Models;
using Internal.Integration.Core.Models.Main;
using Internal.Integration.Core.Models.Main.Sale;

namespace Internal.Integration.Core.Interfaces.Main;

public interface ISaleNode
{
    Task<InternalResponse<InternalFullSale>> GetFullSale(Guid saleId, CancellationToken cancellationToken = default);
}
