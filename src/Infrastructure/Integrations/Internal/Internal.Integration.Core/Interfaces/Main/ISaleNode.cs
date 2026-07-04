using Integrations.Common;
using Internal.Integration.Core.Models.Main.Sale;

namespace Internal.Integration.Core.Interfaces.Main;

public interface ISaleNode
{
    Task<Response<InternalFullSale>> GetFullSale(Guid saleId, CancellationToken cancellationToken = default);
}